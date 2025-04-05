using Cysharp.Threading.Tasks;
using Inventonater.Chromecast.Models;
using Inventonater.Chromecast.Models.ChromecastStatus;
using Inventonater.Chromecast.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Inventonater.Chromecast.Unity
{
    /// <summary>
    /// Unity MonoBehaviour for managing Chromecast connections and operations.
    /// Add this component to a GameObject to interact with Chromecast devices.
    /// </summary>
    public class ChromecastManager : MonoBehaviour
    {
        /// <summary>
        /// Configuration for the Chromecast manager
        /// </summary>
        [Serializable]
        public class ChromecastManagerConfig
        {
            /// <summary>
            /// Default application ID to launch
            /// </summary>
            [Tooltip("Default Chromecast application ID to launch")]
            public string DefaultAppId = "CC1AD845"; // Default media receiver
            
            /// <summary>
            /// Whether to automatically discover devices on start
            /// </summary>
            [Tooltip("Whether to automatically discover devices when the component starts")]
            public bool AutoDiscoverOnStart = true;
            
            /// <summary>
            /// Whether to auto-connect to the first device found
            /// </summary>
            [Tooltip("Whether to automatically connect to the first device found during discovery")]
            public bool AutoConnectToFirstDevice = false;
            
            /// <summary>
            /// Whether to save discovered devices between sessions
            /// </summary>
            [Tooltip("Whether to save discovered devices between sessions")]
            public bool RememberDevices = true;
            
            /// <summary>
            /// Whether to log debug messages
            /// </summary>
            [Tooltip("Whether to log debug messages to the console")]
            public bool DebugLogging = true;
            
            /// <summary>
            /// Subnet scan timeout (milliseconds)
            /// </summary>
            [Tooltip("Timeout in milliseconds for subnet scanning")]
            public int ScanTimeoutMs = 5000;
            
            /// <summary>
            /// Custom IP addresses to scan
            /// </summary>
            [Tooltip("Custom IP addresses to scan (optional)")]
            public List<string> CustomIpAddresses = new List<string>();
        }
        
        /// <summary>
        /// Configuration for the Chromecast manager
        /// </summary>
        [SerializeField]
        public ChromecastManagerConfig Configuration = new ChromecastManagerConfig();
        
        /// <summary>
        /// Event for when devices are discovered
        /// </summary>
        public event Action<IEnumerable<ChromecastReceiver>> OnDevicesDiscovered;
        
        /// <summary>
        /// Event for when connected to a device
        /// </summary>
        public event Action<ChromecastReceiver> OnConnected;
        
        /// <summary>
        /// Event for when disconnected from a device
        /// </summary>
        public event Action OnDisconnected;
        
        /// <summary>
        /// Event for when an application is launched
        /// </summary>
        public event Action<ChromecastApplication> OnApplicationLaunched;
        
        /// <summary>
        /// Event for when media is loaded
        /// </summary>
        public event Action<MediaStatus> OnMediaLoaded;
        
        /// <summary>
        /// Event for when the media status changes
        /// </summary>
        public event Action<MediaStatus> OnMediaStatusChanged;
        
        /// <summary>
        /// Event for when an error occurs
        /// </summary>
        public event Action<string> OnError;
        
        private UnityChromecastLocator _locator;
        private UnityChromecastClient _client;
        private CancellationTokenSource _cts;
        private bool _isInitialized = false;
        private bool _isConnected = false;
        
        /// <summary>
        /// Gets the list of discovered devices
        /// </summary>
        public IEnumerable<ChromecastReceiver> DiscoveredDevices => _locator?.GetKnownDevices() ?? new List<ChromecastReceiver>();
        
        /// <summary>
        /// Gets the currently connected device
        /// </summary>
        public ChromecastReceiver ConnectedDevice { get; private set; }
        
        /// <summary>
        /// Gets the Chromecast status
        /// </summary>
        public ChromecastStatus ChromecastStatus => _client?.GetChromecastStatus();
        
        /// <summary>
        /// Gets the media status
        /// </summary>
        public MediaStatus MediaStatus => _client?.GetMediaStatus();
        
        /// <summary>
        /// Gets whether the manager is connected to a device
        /// </summary>
        public bool IsConnected => _isConnected && _client != null;
        
        /// <summary>
        /// Gets whether the manager is currently running a discovery operation
        /// </summary>
        public bool IsDiscovering { get; private set; }
        
        private void Awake()
        {
            Initialize();
        }
        
        private void OnDestroy()
        {
            _cts?.Cancel();
            DisconnectAsync().Forget();
        }
        
        /// <summary>
        /// Initializes the Chromecast manager
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
                return;
                
            _cts = new CancellationTokenSource();
            
            // Create the discovery config from our settings
            var discoveryConfig = new UnityChromecastLocator.DiscoveryConfig
            {
                CustomIpAddresses = Configuration.CustomIpAddresses,
                ConnectionTimeoutMs = 500
            };
            
            _locator = new UnityChromecastLocator(discoveryConfig);
            _client = new UnityChromecastClient();
            
            // Subscribe to client events
            _client.Disconnected += Client_Disconnected;
            
            // Load any saved devices
            if (Configuration.RememberDevices)
            {
                LoadSavedDevices();
            }
            
            _isInitialized = true;
            
            // Auto-discover if configured
            if (Configuration.AutoDiscoverOnStart)
            {
                UniTask.Create(async () =>
                {
                    await UniTask.Delay(500); // Small delay to ensure everything is initialized
                    await DiscoverDevicesAsync();
                    
                    // Auto-connect if configured
                    if (Configuration.AutoConnectToFirstDevice && DiscoveredDevices.Any())
                    {
                        await ConnectToDeviceAsync(DiscoveredDevices.First());
                    }
                }).Forget();
            }
        }
        
        /// <summary>
        /// Discovers Chromecast devices on the network
        /// </summary>
        /// <returns>The discovered devices</returns>
        public async UniTask<IEnumerable<ChromecastReceiver>> DiscoverDevicesAsync()
        {
            if (!_isInitialized)
                Initialize();
                
            if (IsDiscovering)
            {
                LogDebug("Discovery already in progress");
                return DiscoveredDevices;
            }
            
            IsDiscovering = true;
            IEnumerable<ChromecastReceiver> devices = new List<ChromecastReceiver>();
            
            try
            {
                LogDebug("Starting device discovery");
                
                // Create a new cancellation token source for this operation
                using (var discoveryCts = new CancellationTokenSource(Configuration.ScanTimeoutMs))
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(discoveryCts.Token, _cts.Token))
                {
                    // Discover devices
                    devices = await _locator.FindReceiversAsync(linkedCts.Token);
                    
                    // Save devices if configured
                    if (Configuration.RememberDevices)
                    {
                        SaveDevices(devices);
                    }
                    
                    // Raise event
                    OnDevicesDiscovered?.Invoke(devices);
                }
            }
            catch (OperationCanceledException)
            {
                LogDebug("Device discovery was cancelled or timed out");
            }
            catch (Exception ex)
            {
                LogError($"Error discovering devices: {ex.Message}");
                OnError?.Invoke($"Discovery error: {ex.Message}");
            }
            finally
            {
                IsDiscovering = false;
            }
            
            return devices;
        }
        
        /// <summary>
        /// Connects to a Chromecast device
        /// </summary>
        /// <param name="device">The device to connect to</param>
        /// <returns>The Chromecast status</returns>
        public async UniTask<ChromecastStatus> ConnectToDeviceAsync(ChromecastReceiver device)
        {
            if (!_isInitialized)
                Initialize();
                
            if (_isConnected)
            {
                LogDebug("Already connected to a device, disconnecting first");
                await DisconnectAsync();
            }
            
            try
            {
                LogDebug($"Connecting to device: {device.Name} at {device.DeviceUri.Host}");
                
                var status = await _client.ConnectChromecast(device);
                _isConnected = true;
                ConnectedDevice = device;
                
                // Subscribe to client events
                if (_client.MediaChannel != null)
                {
                    _client.MediaChannel.StatusChanged += MediaChannel_StatusChanged;
                }
                
                // Raise event
                OnConnected?.Invoke(device);
                
                return status;
            }
            catch (Exception ex)
            {
                LogError($"Error connecting to device: {ex.Message}");
                OnError?.Invoke($"Connection error: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Disconnects from the current device
        /// </summary>
        public async UniTask DisconnectAsync()
        {
            if (!_isConnected || _client == null)
                return;
                
            try
            {
                // Unsubscribe from events
                if (_client.MediaChannel != null)
                {
                    _client.MediaChannel.StatusChanged -= MediaChannel_StatusChanged;
                }
                
                await _client.DisconnectAsync();
            }
            catch (Exception ex)
            {
                LogError($"Error disconnecting: {ex.Message}");
            }
            finally
            {
                _isConnected = false;
                ConnectedDevice = null;
                
                // Raise event
                OnDisconnected?.Invoke();
            }
        }
        
        /// <summary>
        /// Launches an application on the connected device
        /// </summary>
        /// <param name="appId">The application ID to launch (optional, uses default if not specified)</param>
        /// <returns>The Chromecast status</returns>
        public async UniTask<ChromecastStatus> LaunchApplicationAsync(string appId = null)
        {
            if (!EnsureConnected())
                throw new InvalidOperationException("Not connected to a device");
                
            try
            {
                appId = appId ?? Configuration.DefaultAppId;
                LogDebug($"Launching application: {appId}");
                
                var status = await _client.LaunchApplicationAsync(appId);
                
                // Raise event if we have an active application
                if (status?.Applications?.Count > 0)
                {
                    OnApplicationLaunched?.Invoke(status.Applications[0]);
                }
                
                return status;
            }
            catch (Exception ex)
            {
                LogError($"Error launching application: {ex.Message}");
                OnError?.Invoke($"Launch error: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Loads and plays media on the connected device
        /// </summary>
        /// <param name="url">The media URL</param>
        /// <param name="contentType">The content type (optional, auto-detected if not specified)</param>
        /// <param name="title">The media title (optional)</param>
        /// <param name="subtitle">The media subtitle (optional)</param>
        /// <param name="streamType">The stream type (default: Buffered)</param>
        /// <returns>The media status</returns>
        public async UniTask<MediaStatus> LoadMediaAsync(string url, string contentType = null, 
            string title = null, string subtitle = null, StreamType streamType = StreamType.Buffered)
        {
            if (!EnsureConnected())
                throw new InvalidOperationException("Not connected to a device");
                
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("URL cannot be empty", nameof(url));
                
            try
            {
                // Detect content type if not specified
                if (string.IsNullOrEmpty(contentType))
                {
                    contentType = GetContentTypeFromUrl(url);
                }
                
                LogDebug($"Loading media: {url} ({contentType})");
                
                // Create media
                var media = new Media
                {
                    ContentUrl = url,
                    ContentType = contentType,
                    StreamType = streamType
                };
                
                // Add metadata if provided
                if (!string.IsNullOrEmpty(title) || !string.IsNullOrEmpty(subtitle))
                {
                    media.Metadata = new MediaMetadata
                    {
                        MetadataType = MetadataType.Generic,
                        Title = title,
                        Subtitle = subtitle
                    };
                }
                
                // Load media
                var status = await _client.MediaChannel.LoadAsync(media);
                
                // Raise event
                OnMediaLoaded?.Invoke(status);
                
                return status;
            }
            catch (Exception ex)
            {
                LogError($"Error loading media: {ex.Message}");
                OnError?.Invoke($"Media error: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Plays the current media
        /// </summary>
        /// <returns>The media status</returns>
        public async UniTask<MediaStatus> PlayAsync()
        {
            if (!EnsureConnected())
                throw new InvalidOperationException("Not connected to a device");
                
            try
            {
                LogDebug("Playing media");
                return await _client.MediaChannel.PlayAsync();
            }
            catch (Exception ex)
            {
                LogError($"Error playing media: {ex.Message}");
                OnError?.Invoke($"Play error: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Pauses the current media
        /// </summary>
        /// <returns>The media status</returns>
        public async UniTask<MediaStatus> PauseAsync()
        {
            if (!EnsureConnected())
                throw new InvalidOperationException("Not connected to a device");
                
            try
            {
                LogDebug("Pausing media");
                return await _client.MediaChannel.PauseAsync();
            }
            catch (Exception ex)
            {
                LogError($"Error pausing media: {ex.Message}");
                OnError?.Invoke($"Pause error: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Stops the current media
        /// </summary>
        /// <returns>The media status</returns>
        public async UniTask<MediaStatus> StopAsync()
        {
            if (!EnsureConnected())
                throw new InvalidOperationException("Not connected to a device");
                
            try
            {
                LogDebug("Stopping media");
                return await _client.MediaChannel.StopAsync();
            }
            catch (Exception ex)
            {
                LogError($"Error stopping media: {ex.Message}");
                OnError?.Invoke($"Stop error: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Seeks to a position in the current media
        /// </summary>
        /// <param name="seconds">The position in seconds</param>
        /// <returns>The media status</returns>
        public async UniTask<MediaStatus> SeekAsync(double seconds)
        {
            if (!EnsureConnected())
                throw new InvalidOperationException("Not connected to a device");
                
            try
            {
                LogDebug($"Seeking to {seconds} seconds");
                return await _client.MediaChannel.SeekAsync(seconds);
            }
            catch (Exception ex)
            {
                LogError($"Error seeking media: {ex.Message}");
                OnError?.Invoke($"Seek error: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Sets the volume level
        /// </summary>
        /// <param name="level">The volume level (0.0 - 1.0)</param>
        /// <returns>The Chromecast status</returns>
        public async UniTask<ChromecastStatus> SetVolumeAsync(float level)
        {
            if (!EnsureConnected())
                throw new InvalidOperationException("Not connected to a device");
                
            try
            {
                // Clamp volume level
                level = Mathf.Clamp01(level);
                LogDebug($"Setting volume to {level}");
                
                return await _client.ReceiverChannel.SetVolumeAsync(level);
            }
            catch (Exception ex)
            {
                LogError($"Error setting volume: {ex.Message}");
                OnError?.Invoke($"Volume error: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Sets the mute state
        /// </summary>
        /// <param name="muted">Whether to mute the device</param>
        /// <returns>The Chromecast status</returns>
        public async UniTask<ChromecastStatus> SetMuteAsync(bool muted)
        {
            if (!EnsureConnected())
                throw new InvalidOperationException("Not connected to a device");
                
            try
            {
                LogDebug($"Setting mute to {muted}");
                return await _client.ReceiverChannel.SetMuteAsync(muted);
            }
            catch (Exception ex)
            {
                LogError($"Error setting mute: {ex.Message}");
                OnError?.Invoke($"Mute error: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Adds a custom IP address to scan
        /// </summary>
        /// <param name="ipAddress">The IP address</param>
        public void AddCustomIpAddress(string ipAddress)
        {
            if (!Configuration.CustomIpAddresses.Contains(ipAddress))
            {
                Configuration.CustomIpAddresses.Add(ipAddress);
            }
        }
        
        /// <summary>
        /// Adds a known device
        /// </summary>
        /// <param name="name">The device name</param>
        /// <param name="ipAddress">The IP address</param>
        /// <param name="port">The port (default: 8009)</param>
        /// <returns>The added device</returns>
        public ChromecastReceiver AddKnownDevice(string name, string ipAddress, int port = 8009)
        {
            if (!_isInitialized)
                Initialize();
                
            return _locator.AddDevice(name, ipAddress, port);
        }
        
        private void Client_Disconnected(object sender, EventArgs e)
        {
            // Run on UI thread
            UniTask.RunOnUiThread(() =>
            {
                _isConnected = false;
                ConnectedDevice = null;
                OnDisconnected?.Invoke();
            }).Forget();
        }
        
        private void MediaChannel_StatusChanged(object sender, MediaStatus e)
        {
            OnMediaStatusChanged?.Invoke(e);
        }
        
        private bool EnsureConnected()
        {
            // Initialize if not initialized
            if (!_isInitialized)
                Initialize();
                
            // Return connection state
            return _isConnected && _client != null;
        }
        
        private string GetContentTypeFromUrl(string url)
        {
            // Simple content type detection based on URL extension
            string extension = System.IO.Path.GetExtension(url).ToLower();
            
            switch (extension)
            {
                case ".mp4":
                case ".m4v":
                    return "video/mp4";
                case ".mp3":
                    return "audio/mp3";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".webm":
                    return "video/webm";
                case ".m3u8":
                    return "application/x-mpegURL";
                case ".mpd":
                    return "application/dash+xml";
                default:
                    return "video/mp4"; // Default to MP4
            }
        }
        
        private void SaveDevices(IEnumerable<ChromecastReceiver> devices)
        {
            // In a real implementation, this would save devices to PlayerPrefs
            // Skipping actual implementation for this example
        }
        
        private void LoadSavedDevices()
        {
            // In a real implementation, this would load devices from PlayerPrefs
            // Skipping actual implementation for this example
        }
        
        private void LogDebug(string message)
        {
            if (Configuration.DebugLogging)
            {
                Debug.Log($"[ChromecastManager] {message}");
            }
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[ChromecastManager] {message}");
        }
    }
}
