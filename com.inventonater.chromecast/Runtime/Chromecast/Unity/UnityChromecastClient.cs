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
        
        private Dictionary<string, Type> RegisterMessageTypes()
        {
            // In a real implementation, we would scan the assembly for message types
            // with the ReceptionMessageAttribute, similar to what the original does
            
            // For now, this is a simplified placeholder
            var types = new Dictionary<string, Type>();
            
            // TODO: Register all message types here
            // Example: types.Add("RECEIVER_STATUS", typeof(ReceiverStatusMessage));
            
            return types;
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
                    // TODO: Implement the receive loop
                    // This would read messages from the stream, deserialize them,
                    // and dispatch them to the appropriate channel
                    
                    // For now, just a placeholder
                    await UniTask.Delay(100, cancellationToken: _receiveCts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in receive loop: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a message to the Chromecast device
        /// </summary>
        public async UniTask SendAsync(ILogger channelLogger, string ns, IMessage message, string destinationId)
        {
            // TODO: Implement the send method
            // This would serialize the message and send it over the stream
            
            Debug.Log($"Sending message to {destinationId}: {message.Type}");
            await UniTask.CompletedTask;
        }

        /// <summary>
        /// Sends a message to the Chromecast device and waits for a response
        /// </summary>
        public async UniTask<TResponse> SendAsync<TResponse>(ILogger channelLogger, string ns, IMessageWithId message, string destinationId) where TResponse : IMessageWithId
        {
            // TODO: Implement the send method with response
            // This would send the message and set up a TaskCompletionSource to wait for the response
            
            Debug.Log($"Sending message with ID {message.RequestId} to {destinationId}: {message.Type}");
            
            // For now, return a dummy response
            await UniTask.Delay(100);
            return default;
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
