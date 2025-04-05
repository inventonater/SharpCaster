using Cysharp.Threading.Tasks;
using Inventonater.Chromecast.Channels;
using Inventonater.Chromecast.Interfaces;
using Inventonater.Chromecast.Messages;
using Inventonater.Chromecast.Models;
using Inventonater.Chromecast.Models.ChromecastStatus;
using Inventonater.Chromecast.Models.Media;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Inventonater.Chromecast.Unity
{
    /// <summary>
    /// Unity-compatible implementation of the Chromecast client
    /// </summary>
    public class UnityChromecastClient : IChromecastClient
    {
        private const int RECEIVE_TIMEOUT = 30000;

        /// <summary>
        /// Raised when the sender is disconnected
        /// </summary>
        public event EventHandler Disconnected;
        
        /// <summary>
        /// Gets the sender ID
        /// </summary>
        public Guid SenderId { get; } = Guid.NewGuid();
        
        /// <summary>
        /// Gets or sets the friendly name
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets the media channel
        /// </summary>
        public IMediaChannel MediaChannel => GetChannel<IMediaChannel>();
        
        /// <summary>
        /// Gets the heartbeat channel
        /// </summary>
        public IHeartbeatChannel HeartbeatChannel => GetChannel<IHeartbeatChannel>();
        
        /// <summary>
        /// Gets the receiver channel
        /// </summary>
        public IReceiverChannel ReceiverChannel => GetChannel<IReceiverChannel>();
        
        /// <summary>
        /// Gets the connection channel
        /// </summary>
        public IConnectionChannel ConnectionChannel => GetChannel<IConnectionChannel>();

        private ILogger _logger = null;
        private TcpClient _client;
        private Stream _stream;
        private CancellationTokenSource _receiveCts;
        private SemaphoreSlim _sendSemaphoreSlim = new SemaphoreSlim(1, 1);

        private IDictionary<string, Type> _messageTypes;
        private IEnumerable<IChromecastChannel> _channels;
        private ConcurrentDictionary<int, object> _waitingTasks = new ConcurrentDictionary<int, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityChromecastClient"/> class
        /// </summary>
        public UnityChromecastClient()
        {
            // Create a lightweight service collection/provider
            var loggerFactory = new Unity.SimpleLoggerFactory();
            
            var serviceProvider = new Unity.SimpleServiceProvider();
            
            // Register channels
            serviceProvider.RegisterService<IChromecastChannel>(new ConnectionChannel());
            serviceProvider.RegisterService<IChromecastChannel>(new HeartbeatChannel());
            serviceProvider.RegisterService<IChromecastChannel>(new ReceiverChannel());
            serviceProvider.RegisterService<IChromecastChannel>(new MediaChannel());
            
            // TODO: Register message types 
            // This would normally scan the assembly for IMessage types with ReceptionMessageAttribute
            // For now, we'll manually register the most important ones in a separate method
            _messageTypes = RegisterMessageTypes();
            
            _channels = serviceProvider.GetServices<IChromecastChannel>();
            
            foreach (var channel in _channels)
            {
                channel.Client = this;
            }
        }
        
        /// <summary>
        /// Register message types by scanning for classes with ReceptionMessageAttribute
        /// or manually registering known types
        /// </summary>
        private Dictionary<string, Type> RegisterMessageTypes()
        {
            var types = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            
            try
            {
                // Method 1: Scan assemblies for message types with ReceptionMessageAttribute
                var assembly = typeof(Message).Assembly;
                var messageTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && typeof(IMessage).IsAssignableFrom(t));
                
                foreach (var type in messageTypes)
                {
                    var attr = type.GetCustomAttribute<ReceptionMessageAttribute>();
                    if (attr != null && !string.IsNullOrEmpty(attr.Type))
                    {
                        try
                        {
                            types[attr.Type] = type;
                            Debug.Log($"Auto-registered message type: {attr.Type} -> {type.Name}");
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error registering message type {type.Name}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Could not automatically scan for message types: {ex.Message}");
                Debug.LogWarning("Falling back to manual registration");
            }
            
            // Method 2: Manual registration for critical types (as a fallback)
            RegisterMessageTypeIfNotExists(types, "RECEIVER_STATUS", typeof(Messages.Receiver.ReceiverStatusMessage));
            RegisterMessageTypeIfNotExists(types, "CLOSE", typeof(Messages.Connection.CloseMessage));
            RegisterMessageTypeIfNotExists(types, "PING", typeof(Messages.Heartbeat.PingMessage));
            RegisterMessageTypeIfNotExists(types, "PONG", typeof(Messages.Heartbeat.PongMessage));
            
            Debug.Log($"Registered {types.Count} message types in total");
            return types;
        }
        
        /// <summary>
        /// Helper method to register a message type if it doesn't already exist
        /// </summary>
        private void RegisterMessageTypeIfNotExists(Dictionary<string, Type> types, string key, Type type)
        {
            if (!types.ContainsKey(key))
            {
                types[key] = type;
                Debug.Log($"Manually registered message type: {key} -> {type.Name}");
            }
        }
        
        /// <summary>
        /// Register a message type manually
        /// </summary>
        public void RegisterMessageType(string type, Type messageType)
        {
            if (_messageTypes.ContainsKey(type))
            {
                _messageTypes[type] = messageType;
            }
            else
            {
                _messageTypes.Add(type, messageType);
            }
        }

        /// <summary>
        /// Connects to a Chromecast device
        /// </summary>
        /// <param name="chromecastReceiver">The Chromecast receiver to connect to</param>
        /// <returns>The Chromecast status</returns>
        public async UniTask<ChromecastStatus> ConnectChromecast(ChromecastReceiver chromecastReceiver)
        {
            await DisconnectAsync();
            FriendlyName = chromecastReceiver.Name;
            
            Debug.Log($"Connecting to Chromecast: {FriendlyName}");
            
            try
            {
                _client = new TcpClient();
                await UniTask.RunOnThreadPool(() => 
                    _client.Connect(chromecastReceiver.DeviceUri.Host, chromecastReceiver.Port));
                
                // Open SSL stream to Chromecast and bypass SSL validation
                var secureStream = new SslStream(_client.GetStream(), true, 
                    (_, __, ___, ____) => true);
                
                await UniTask.RunOnThreadPool(() => 
                    secureStream.AuthenticateAsClientAsync(chromecastReceiver.DeviceUri.Host));
                
                _stream = secureStream;
                
                _receiveCts = new CancellationTokenSource();
                StartReceiving();
                
                HeartbeatChannel.StartTimeoutTimer();
                HeartbeatChannel.StatusChanged += HeartBeatTimedOut;
                
                await ConnectionChannel.ConnectAsync();
                return await ReceiverChannel.GetChromecastStatusAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error connecting to Chromecast: {ex.Message}");
                await DisconnectAsync();
                throw;
            }
        }
        
        private async void HeartBeatTimedOut(object sender, EventArgs e)
        {
            Debug.LogError("Heartbeat timeout - Disconnecting client.");
            await DisconnectAsync();
        }
        
        private void StartReceiving()
        {
            UniTask.RunOnThreadPool(ReceiveLoop).Forget();
        }
        
        private async UniTask ReceiveLoop()
        {
            try
            {
                while (!_receiveCts.IsCancellationRequested)
                {
                    try
                    {
                        var message = await ReadMessageAsync();
                        if (message == null)
                        {
                            // If we get a null message, the connection is likely closed
                            Debug.LogWarning("Received null message, connection may be closed");
                            break;
                        }

                        // Find the appropriate channel for this message's namespace
                        var channel = _channels.FirstOrDefault(x => x.Namespace == message.Namespace);
                        
                        if (channel != null)
                        {
                            try
                            {
                                // If the payload is JSON, try to deserialize it into the appropriate message type
                                if (message.PayloadType == Extensions.Api.CastChannel.CastMessage.Types.PayloadType.String)
                                {
                                    // Get the JSON payload
                                    string json = message.PayloadUtf8;
                                    
                                    // Try to get the message type
                                    string messageType = message.GetJsonType();
                                    
                                    // Find the appropriate message type class for this message type
                                    if (_messageTypes.TryGetValue(messageType, out Type messageClass))
                                    {
                                        // Deserialize the JSON into the message class
                                        var parsedMessage = JsonConvert.DeserializeObject(json, messageClass) as IMessage;
                                        
                                        // If the message has an ID and we're waiting for it
                                        if (parsedMessage is IMessageWithId messageWithId)
                                        {
                                            // Try to find a waiting task for this message ID
                                            if (_waitingTasks.TryRemove(messageWithId.RequestId, out var taskObj))
                                            {
                                                // If we have a task waiting for this message, complete it
                                                var tc = taskObj as dynamic;
                                                tc.TrySetResult(parsedMessage);
                                                continue;
                                            }
                                        }
                                        
                                        // Otherwise, dispatch to the channel
                                        await channel.OnMessageReceivedAsync(parsedMessage);
                                    }
                                    else
                                    {
                                        // Unknown message type, but still let the channel handle it as raw JSON
                                        Debug.LogWarning($"Unknown message type: {messageType} - payload: {json}");
                                        await channel.OnMessageReceivedAsync(new Message { PayloadJson = json });
                                    }
                                }
                            }
                            catch (Exception channelEx)
                            {
                                Debug.LogError($"Error processing message in channel: {channelEx}");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"No channel found for namespace: {message.Namespace}");
                        }
                    }
                    catch (Exception readEx)
                    {
                        if (_receiveCts.IsCancellationRequested)
                            break;
                            
                        Debug.LogError($"Error reading message: {readEx}");
                        await UniTask.Delay(100, cancellationToken: _receiveCts.Token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                Debug.Log("Receive loop cancelled");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in receive loop: {ex.Message}");
            }
        }
        
        private async UniTask<Extensions.Api.CastChannel.CastMessage> ReadMessageAsync()
        {
            // Chromecast messages are prefixed with a 4-byte big-endian length
            var headerBuffer = new byte[4];
            
            try
            {
                // Read the message length header
                int bytesRead = await UniTask.RunOnThreadPool(() =>
                    _stream.ReadAsync(headerBuffer, 0, 4));
                    
                if (bytesRead < 4)
                {
                    Debug.LogWarning($"Failed to read message header: got {bytesRead} bytes");
                    return null;
                }
                
                // Convert the big-endian header to a message length
                int messageLength = (headerBuffer[0] << 24) | (headerBuffer[1] << 16) | 
                                    (headerBuffer[2] << 8) | headerBuffer[3];
                                    
                var messageBuffer = new byte[messageLength];
                
                // Read the full message
                bytesRead = await UniTask.RunOnThreadPool(() => 
                    _stream.ReadAsync(messageBuffer, 0, messageLength));
                
                if (bytesRead < messageLength)
                {
                    Debug.LogWarning($"Failed to read full message: got {bytesRead} of {messageLength} bytes");
                    return null;
                }
                
                // Parse the protocol buffer message
                var message = Extensions.Api.CastChannel.CastMessage.Parser.ParseFrom(messageBuffer);
                return message;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading message: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Sends a message to the Chromecast device
        /// </summary>
        public async UniTask SendAsync(ILogger channelLogger, string ns, IMessage message, string destinationId)
        {
            await _sendSemaphoreSlim.WaitAsync();
            
            try
            {
                channelLogger?.LogDebug($"Sending message to {destinationId}: {message.Type}");
                Debug.Log($"Sending message to {destinationId}: {message.Type}");
                
                var castMessage = new Extensions.Api.CastChannel.CastMessage
                {
                    ProtocolVersion = Extensions.Api.CastChannel.CastMessage.Types.ProtocolVersion.Castv21,
                    SourceId = SenderId.ToString(),
                    DestinationId = destinationId,
                    Namespace = ns,
                    PayloadType = Extensions.Api.CastChannel.CastMessage.Types.PayloadType.String
                };
                
                // Serialize the message to JSON
                string json = JsonConvert.SerializeObject(message);
                castMessage.PayloadUtf8 = json;
                
                // Convert to bytes
                var data = castMessage.ToProto();
                
                // Send the message over the stream
                await UniTask.RunOnThreadPool(() => _stream.WriteAsync(data, 0, data.Length));
                await UniTask.RunOnThreadPool(() => _stream.FlushAsync());
            }
            catch (Exception ex)
            {
                channelLogger?.LogError($"Error sending message: {ex.Message}");
                Debug.LogError($"Error sending message: {ex.Message}");
                throw;
            }
            finally
            {
                _sendSemaphoreSlim.Release();
            }
        }

        /// <summary>
        /// Sends a message to the Chromecast device and waits for a response
        /// </summary>
        public async UniTask<TResponse> SendAsync<TResponse>(ILogger channelLogger, string ns, IMessageWithId message, string destinationId) where TResponse : IMessageWithId
        {
            // Create a completion source for the response
            var tcs = new UniTaskCompletionSource<TResponse>();
            
            try
            {
                // Store the completion source for this request ID
                _waitingTasks[message.RequestId] = tcs;
                
                // Send the message
                await SendAsync(channelLogger, ns, message, destinationId);
                
                // Wait for the response with a timeout
                return await tcs.Task.Timeout(TimeSpan.FromMilliseconds(RECEIVE_TIMEOUT));
            }
            catch (TimeoutException)
            {
                channelLogger?.LogError($"Timeout waiting for response to message {message.Type} with ID {message.RequestId}");
                Debug.LogError($"Timeout waiting for response to message {message.Type} with ID {message.RequestId}");
                throw;
            }
            finally
            {
                // Clean up the waiting task
                _waitingTasks.TryRemove(message.RequestId, out _);
            }
        }

        /// <summary>
        /// Disconnects from the Chromecast device
        /// </summary>
        public async UniTask DisconnectAsync()
        {
            foreach (var channel in GetStatusChannels())
            {
                var statusProperty = channel.GetType().GetProperty("Status");
                if (statusProperty != null)
                {
                    statusProperty.SetValue(channel, null);
                }
            }
            
            if (HeartbeatChannel != null)
            {
                HeartbeatChannel.StopTimeoutTimer();
                HeartbeatChannel.StatusChanged -= HeartBeatTimedOut;
            }
            
            _receiveCts?.Cancel();
            
            if (_client != null)
            {
                _waitingTasks.Clear();
                
                if (_stream != null)
                {
                    await UniTask.RunOnThreadPool(() => {
                        try { _stream.Dispose(); } catch { }
                    });
                    _stream = null;
                }
                
                await UniTask.RunOnThreadPool(() => {
                    try { _client.Dispose(); } catch { }
                });
                _client = null;
                
                OnDisconnected();
            }
        }
        
        /// <summary>
        /// Raises the Disconnected event
        /// </summary>
        protected virtual void OnDisconnected()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets a channel
        /// </summary>
        /// <typeparam name="TChannel">The channel type</typeparam>
        /// <returns>The channel</returns>
        public TChannel GetChannel<TChannel>() where TChannel : IChromecastChannel
        {
            return _channels.OfType<TChannel>().FirstOrDefault();
        }

        /// <summary>
        /// Launches an application on the Chromecast device
        /// </summary>
        public async UniTask<ChromecastStatus> LaunchApplicationAsync(string applicationId, bool joinExistingApplicationSession = true)
        {
            if (joinExistingApplicationSession)
            {
                var status = GetChromecastStatus();
                var runningApplication = status?.Applications?.FirstOrDefault(x => x.AppId == applicationId);
                
                if (runningApplication != null)
                {
                    await ConnectionChannel.ConnectAsync(runningApplication.TransportId);
                    
                    // Check if the application is using the media namespace
                    if (runningApplication.Namespaces.Any(ns => ns.Name == "urn:x-cast:com.google.cast.media"))
                    {
                        await MediaChannel.GetMediaStatusAsync();
                    }
                    
                    return await ReceiverChannel.GetChromecastStatusAsync();
                }
            }
            
            var newApplication = await ReceiverChannel.LaunchApplicationAsync(applicationId);
            await ConnectionChannel.ConnectAsync(newApplication.Application.TransportId);
            return await ReceiverChannel.GetChromecastStatusAsync();
        }
        
        private IEnumerable<IChromecastChannel> GetStatusChannels()
        {
            var statusChannelType = typeof(IStatusChannel<>);
            return _channels.Where(c => c.GetType().GetInterfaces().Any(i => 
                i.GetTypeInfo().IsGenericType && 
                i.GetGenericTypeDefinition() == statusChannelType));
        }

        /// <summary>
        /// Gets the different statuses
        /// </summary>
        public IDictionary<string, object> GetStatuses()
        {
            return GetStatusChannels().ToDictionary(
                c => c.Namespace, 
                c => c.GetType().GetProperty("Status").GetValue(c));
        }

        /// <summary>
        /// Gets the Chromecast status
        /// </summary>
        public ChromecastStatus GetChromecastStatus()
        {
            return ReceiverChannel?.Status;
        }

        /// <summary>
        /// Gets the media status
        /// </summary>
        public MediaStatus GetMediaStatus()
        {
            return MediaChannel?.Status;
        }
    }
}
