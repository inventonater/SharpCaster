using Cysharp.Threading.Tasks;
using Inventonater.Chromecast.Models;
using Inventonater.Chromecast.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Inventonater.Chromecast.Unity
{
    /// <summary>
    /// Unity MonoBehaviour for managing Chromecast functionality
    /// </summary>
    public class ChromecastManager : MonoBehaviour
    {
        /// <summary>
        /// Event raised when Chromecast devices are discovered
        /// </summary>
        public event Action<IEnumerable<ChromecastReceiver>> OnDevicesDiscovered;
        
        /// <summary>
        /// Event raised when connected to a Chromecast device
        /// </summary>
        public event Action<bool> OnConnected;
        
        /// <summary>
        /// Event raised when disconnected from a Chromecast device
        /// </summary>
        public event Action OnDisconnected;
        
        /// <summary>
        /// Event raised when media status is updated
        /// </summary>
        public event Action<MediaStatus> OnMediaStatusUpdated;
        
        private UnityChromecastLocator _locator;
        private UnityChromecastClient _client;
        private List<ChromecastReceiver> _discoveredDevices = new List<ChromecastReceiver>();
        private CancellationTokenSource _discoveryCts;
        
        /// <summary>
        /// Gets a value indicating whether we are connected to a Chromecast device
        /// </summary>
        public bool IsConnected { get; private set; }
        
        /// <summary>
        /// Gets the currently connected device
        /// </summary>
        public ChromecastReceiver ConnectedDevice { get; private set; }
        
        /// <summary>
        /// Gets the media status
        /// </summary>
        public MediaStatus MediaStatus => _client?.GetMediaStatus();
        
        /// <summary>
        /// Gets the Chromecast status
        /// </summary>
        public Models.ChromecastStatus.ChromecastStatus ChromecastStatus => _client?.GetChromecastStatus();

        private void Awake()
        {
            _locator = new UnityChromecastLocator();
            _client = new UnityChromecastClient();
            _client.Disconnected += OnClientDisconnected;
        }
        
        private void OnDestroy()
        {
            DisconnectAsync().Forget();
            _discoveryCts?.Cancel();
            
            if (_client != null)
            {
                _client.Disconnected -= OnClientDisconnected;
            }
        }
        
        private void OnClientDisconnected(object sender, EventArgs e)
        {
            IsConnected = false;
            ConnectedDevice = null;
            OnDisconnected?.Invoke();
        }
        
        /// <summary>
        /// Discovers Chromecast devices on the network
        /// </summary>
        public async UniTask<IEnumerable<ChromecastReceiver>> DiscoverDevicesAsync(int timeoutMs = 5000)
        {
            _discoveryCts?.Cancel();
            _discoveryCts = new CancellationTokenSource();
            _discoveryCts.CancelAfter(timeoutMs);
            
            try
            {
                _discoveredDevices.Clear();
                
                var devices = await _locator.FindReceiversAsync(_discoveryCts.Token);
                
                _discoveredDevices.AddRange(devices);
                OnDevicesDiscovered?.Invoke(_discoveredDevices);
                
                return _discoveredDevices;
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Device discovery was cancelled");
                return _discoveredDevices;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error discovering devices: {ex.Message}");
                return Enumerable.Empty<ChromecastReceiver>();
            }
        }
        
        /// <summary>
        /// Connects to a Chromecast device
        /// </summary>
        public async UniTask<bool> ConnectToDeviceAsync(ChromecastReceiver device)
        {
            if (IsConnected)
            {
                await DisconnectAsync();
            }
            
            try
            {
                Debug.Log($"Connecting to {device.Name}...");
                await _client.ConnectChromecast(device);
                
                IsConnected = true;
                ConnectedDevice = device;
                
                Debug.Log($"Connected to {device.Name}");
                OnConnected?.Invoke(true);
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error connecting to device: {ex.Message}");
                OnConnected?.Invoke(false);
                return false;
            }
        }
        
        /// <summary>
        /// Disconnects from the Chromecast device
        /// </summary>
        public async UniTask DisconnectAsync()
        {
            if (_client != null)
            {
                await _client.DisconnectAsync();
            }
            
            IsConnected = false;
            ConnectedDevice = null;
        }
        
        /// <summary>
        /// Launches an application on the Chromecast device
        /// </summary>
        public async UniTask<bool> LaunchApplicationAsync(string applicationId)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("Not connected to a Chromecast device");
                return false;
            }
            
            try
            {
                var status = await _client.LaunchApplicationAsync(applicationId);
                return status != null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error launching application: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Loads media on the Chromecast device
        /// </summary>
        public async UniTask<bool> LoadMediaAsync(string url, string contentType = null, string title = null, string subtitle = null)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("Not connected to a Chromecast device");
                return false;
            }
            
            try
            {
                var media = new Media
                {
                    ContentUrl = url,
                    ContentType = contentType,
                    StreamType = StreamType.Buffered,
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
                
                var status = await _client.MediaChannel.LoadAsync(media);
                OnMediaStatusUpdated?.Invoke(status);
                return status != null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading media: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Plays the current media
        /// </summary>
        public async UniTask<bool> PlayAsync()
        {
            if (!IsConnected)
            {
                Debug.LogWarning("Not connected to a Chromecast device");
                return false;
            }
            
            try
            {
                var status = await _client.MediaChannel.PlayAsync();
                OnMediaStatusUpdated?.Invoke(status);
                return status != null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error playing media: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Pauses the current media
        /// </summary>
        public async UniTask<bool> PauseAsync()
        {
            if (!IsConnected)
            {
                Debug.LogWarning("Not connected to a Chromecast device");
                return false;
            }
            
            try
            {
                var status = await _client.MediaChannel.PauseAsync();
                OnMediaStatusUpdated?.Invoke(status);
                return status != null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error pausing media: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Stops the current media
        /// </summary>
        public async UniTask<bool> StopAsync()
        {
            if (!IsConnected)
            {
                Debug.LogWarning("Not connected to a Chromecast device");
                return false;
            }
            
            try
            {
                var status = await _client.MediaChannel.StopAsync();
                OnMediaStatusUpdated?.Invoke(status);
                return status != null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error stopping media: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Seeks to a position in the current media
        /// </summary>
        public async UniTask<bool> SeekAsync(double seconds)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("Not connected to a Chromecast device");
                return false;
            }
            
            try
            {
                var status = await _client.MediaChannel.SeekAsync(seconds);
                OnMediaStatusUpdated?.Invoke(status);
                return status != null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error seeking media: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Sets the volume level
        /// </summary>
        public async UniTask<bool> SetVolumeAsync(float level)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("Not connected to a Chromecast device");
                return false;
            }
            
            try
            {
                var status = await _client.ReceiverChannel.SetVolumeAsync(level);
                return status != null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error setting volume: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Sets the mute state
        /// </summary>
        public async UniTask<bool> SetMuteAsync(bool muted)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("Not connected to a Chromecast device");
                return false;
            }
            
            try
            {
                var status = await _client.ReceiverChannel.SetMuteAsync(muted);
                return status != null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error setting mute: {ex.Message}");
                return false;
            }
        }
    }
}
