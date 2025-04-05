using Cysharp.Threading.Tasks;
using Inventonater.Chromecast.Models;
using Inventonater.Chromecast.Models.ChromecastStatus;
using Inventonater.Chromecast.Models.Media;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Inventonater.Chromecast.Unity
{
    /// <summary>
    /// Example MonoBehaviour demonstrating how to use the ChromecastManager
    /// Attach to a GameObject with a ChromecastManager component
    /// </summary>
    public class ChromecastExample : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Reference to a ChromecastManager component")]
        [SerializeField] private ChromecastManager _chromecastManager;
        
        [Header("UI References")]
        [Tooltip("Dropdown to display discovered devices")]
        [SerializeField] private Dropdown _deviceDropdown;
        
        [Tooltip("Button to trigger device discovery")]
        [SerializeField] private Button _discoverButton;
        
        [Tooltip("Button to connect to selected device")]
        [SerializeField] private Button _connectButton;
        
        [Tooltip("Button to disconnect from current device")]
        [SerializeField] private Button _disconnectButton;
        
        [Tooltip("Input field for media URL")]
        [SerializeField] private InputField _mediaUrlInput;
        
        [Tooltip("Button to load and play media")]
        [SerializeField] private Button _playMediaButton;
        
        [Tooltip("Button to pause media")]
        [SerializeField] private Button _pauseButton;
        
        [Tooltip("Button to stop media")]
        [SerializeField] private Button _stopButton;
        
        [Tooltip("Slider for volume control")]
        [SerializeField] private Slider _volumeSlider;
        
        [Tooltip("Status text field")]
        [SerializeField] private Text _statusText;
        
        // List of discovered devices
        private List<ChromecastReceiver> _discoveredDevices = new List<ChromecastReceiver>();
        
        // Currently selected device index
        private int _selectedDeviceIndex = -1;
        
        private void Start()
        {
            // Get reference to ChromecastManager if not set in Inspector
            if (_chromecastManager == null)
            {
                _chromecastManager = GetComponent<ChromecastManager>();
                
                if (_chromecastManager == null)
                {
                    Debug.LogError("ChromecastExample requires a ChromecastManager component");
                    enabled = false;
                    return;
                }
            }
            
            // Initialize UI
            InitializeUI();
            
            // Subscribe to ChromecastManager events
            SubscribeToEvents();
            
            // Update UI state
            UpdateUIState();
            
            // Set default status text
            UpdateStatus("Ready. Click 'Discover Devices' to start.");
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            UnsubscribeFromEvents();
        }
        
        private void InitializeUI()
        {
            // Setup UI event handlers
            if (_discoverButton != null)
                _discoverButton.onClick.AddListener(OnDiscoverButtonClicked);
                
            if (_connectButton != null)
                _connectButton.onClick.AddListener(OnConnectButtonClicked);
                
            if (_disconnectButton != null)
                _disconnectButton.onClick.AddListener(OnDisconnectButtonClicked);
                
            if (_playMediaButton != null)
                _playMediaButton.onClick.AddListener(OnPlayMediaButtonClicked);
                
            if (_pauseButton != null)
                _pauseButton.onClick.AddListener(OnPauseButtonClicked);
                
            if (_stopButton != null)
                _stopButton.onClick.AddListener(OnStopButtonClicked);
                
            if (_deviceDropdown != null)
                _deviceDropdown.onValueChanged.AddListener(OnDeviceDropdownValueChanged);
                
            if (_volumeSlider != null)
            {
                _volumeSlider.minValue = 0f;
                _volumeSlider.maxValue = 1f;
                _volumeSlider.value = 0.5f;
                _volumeSlider.onValueChanged.AddListener(OnVolumeSliderValueChanged);
            }
            
            // Populate device dropdown with any already discovered devices
            PopulateDeviceDropdown(_chromecastManager.DiscoveredDevices);
        }
        
        private void SubscribeToEvents()
        {
            if (_chromecastManager != null)
            {
                _chromecastManager.OnDevicesDiscovered += OnDevicesDiscovered;
                _chromecastManager.OnConnected += OnDeviceConnected;
                _chromecastManager.OnDisconnected += OnDeviceDisconnected;
                _chromecastManager.OnMediaStatusChanged += OnMediaStatusChanged;
                _chromecastManager.OnError += OnError;
            }
        }
        
        private void UnsubscribeFromEvents()
        {
            if (_chromecastManager != null)
            {
                _chromecastManager.OnDevicesDiscovered -= OnDevicesDiscovered;
                _chromecastManager.OnConnected -= OnDeviceConnected;
                _chromecastManager.OnDisconnected -= OnDeviceDisconnected;
                _chromecastManager.OnMediaStatusChanged -= OnMediaStatusChanged;
                _chromecastManager.OnError -= OnError;
            }
        }
        
        #region Button Event Handlers
        
        private void OnDiscoverButtonClicked()
        {
            DiscoverDevicesAsync().Forget();
        }
        
        private void OnConnectButtonClicked()
        {
            if (_selectedDeviceIndex >= 0 && _selectedDeviceIndex < _discoveredDevices.Count)
            {
                ConnectToDeviceAsync(_discoveredDevices[_selectedDeviceIndex]).Forget();
            }
        }
        
        private void OnDisconnectButtonClicked()
        {
            DisconnectAsync().Forget();
        }
        
        private void OnPlayMediaButtonClicked()
        {
            if (_mediaUrlInput != null && !string.IsNullOrEmpty(_mediaUrlInput.text))
            {
                PlayMediaAsync(_mediaUrlInput.text).Forget();
            }
            else
            {
                UpdateStatus("Please enter a media URL");
            }
        }
        
        private void OnPauseButtonClicked()
        {
            PauseMediaAsync().Forget();
        }
        
        private void OnStopButtonClicked()
        {
            StopMediaAsync().Forget();
        }
        
        private void OnDeviceDropdownValueChanged(int index)
        {
            _selectedDeviceIndex = index;
            UpdateUIState();
        }
        
        private void OnVolumeSliderValueChanged(float value)
        {
            SetVolumeAsync(value).Forget();
        }
        
        #endregion
        
        #region ChromecastManager Event Handlers
        
        private void OnDevicesDiscovered(IEnumerable<ChromecastReceiver> devices)
        {
            // Update UI on main thread
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                PopulateDeviceDropdown(devices);
                UpdateStatus($"Found {_discoveredDevices.Count} devices");
                UpdateUIState();
            });
        }
        
        private void OnDeviceConnected(ChromecastReceiver device)
        {
            // Update UI on main thread
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                UpdateStatus($"Connected to {device.Name}");
                UpdateUIState();
            });
        }
        
        private void OnDeviceDisconnected()
        {
            // Update UI on main thread
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                UpdateStatus("Disconnected");
                UpdateUIState();
            });
        }
        
        private void OnMediaStatusChanged(MediaStatus status)
        {
            // Update UI on main thread
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                UpdateMediaStatus(status);
            });
        }
        
        private void OnError(string errorMessage)
        {
            // Update UI on main thread
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                UpdateStatus($"Error: {errorMessage}");
            });
        }
        
        #endregion
        
        #region Async Operations
        
        private async UniTaskVoid DiscoverDevicesAsync()
        {
            UpdateStatus("Discovering devices...");
            UpdateUIState(isDiscovering: true);
            
            try
            {
                await _chromecastManager.DiscoverDevicesAsync();
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"Discovery error: {ex.Message}");
            }
            finally
            {
                UpdateUIState(isDiscovering: false);
            }
        }
        
        private async UniTaskVoid ConnectToDeviceAsync(ChromecastReceiver device)
        {
            UpdateStatus($"Connecting to {device.Name}...");
            UpdateUIState(isConnecting: true);
            
            try
            {
                await _chromecastManager.ConnectToDeviceAsync(device);
                await _chromecastManager.LaunchApplicationAsync();
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"Connection error: {ex.Message}");
            }
            finally
            {
                UpdateUIState(isConnecting: false);
            }
        }
        
        private async UniTaskVoid DisconnectAsync()
        {
            UpdateStatus("Disconnecting...");
            
            try
            {
                await _chromecastManager.DisconnectAsync();
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"Disconnect error: {ex.Message}");
            }
        }
        
        private async UniTaskVoid PlayMediaAsync(string url)
        {
            UpdateStatus($"Loading media: {url}");
            
            try
            {
                await _chromecastManager.LoadMediaAsync(url);
                UpdateStatus("Media loaded and playing");
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"Media error: {ex.Message}");
            }
        }
        
        private async UniTaskVoid PauseMediaAsync()
        {
            try
            {
                await _chromecastManager.PauseAsync();
                UpdateStatus("Media paused");
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"Pause error: {ex.Message}");
            }
        }
        
        private async UniTaskVoid StopMediaAsync()
        {
            try
            {
                await _chromecastManager.StopAsync();
                UpdateStatus("Media stopped");
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"Stop error: {ex.Message}");
            }
        }
        
        private async UniTaskVoid SetVolumeAsync(float volume)
        {
            try
            {
                await _chromecastManager.SetVolumeAsync(volume);
            }
            catch (System.Exception)
            {
                // Ignore volume errors in UI
            }
        }
        
        #endregion
        
        #region UI Helpers
        
        private void PopulateDeviceDropdown(IEnumerable<ChromecastReceiver> devices)
        {
            if (_deviceDropdown == null) return;
            
            _discoveredDevices = devices.ToList();
            
            _deviceDropdown.ClearOptions();
            
            if (_discoveredDevices.Count > 0)
            {
                var options = _discoveredDevices.Select(d => new Dropdown.OptionData(d.Name)).ToList();
                _deviceDropdown.AddOptions(options);
                _deviceDropdown.value = 0;
                _selectedDeviceIndex = 0;
            }
            else
            {
                _deviceDropdown.AddOptions(new List<string> { "No devices found" });
                _selectedDeviceIndex = -1;
            }
        }
        
        private void UpdateUIState(bool isDiscovering = false, bool isConnecting = false)
        {
            if (_discoverButton != null)
                _discoverButton.interactable = !isDiscovering && !isConnecting && !_chromecastManager.IsDiscovering;
                
            if (_connectButton != null)
                _connectButton.interactable = !isDiscovering && !isConnecting && _selectedDeviceIndex >= 0 && !_chromecastManager.IsConnected;
                
            if (_disconnectButton != null)
                _disconnectButton.interactable = _chromecastManager.IsConnected;
                
            if (_playMediaButton != null)
                _playMediaButton.interactable = _chromecastManager.IsConnected;
                
            if (_pauseButton != null)
                _pauseButton.interactable = _chromecastManager.IsConnected && _chromecastManager.MediaStatus != null;
                
            if (_stopButton != null)
                _stopButton.interactable = _chromecastManager.IsConnected && _chromecastManager.MediaStatus != null;
                
            if (_volumeSlider != null)
                _volumeSlider.interactable = _chromecastManager.IsConnected;
                
            if (_mediaUrlInput != null)
                _mediaUrlInput.interactable = _chromecastManager.IsConnected;
        }
        
        private void UpdateStatus(string status)
        {
            if (_statusText != null)
            {
                _statusText.text = status;
                Debug.Log($"[ChromecastExample] {status}");
            }
        }
        
        private void UpdateMediaStatus(MediaStatus status)
        {
            if (status == null) return;
            
            string playerState = status.PlayerState.ToString();
            
            UpdateStatus($"Media {playerState}");
            UpdateUIState();
        }
        
        #endregion
    }

    /// <summary>
    /// Simple dispatcher to run actions on the Unity main thread
    /// </summary>
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static UnityMainThreadDispatcher _instance;
        private readonly Queue<System.Action> _actions = new Queue<System.Action>();
        private readonly object _lockObject = new object();
        
        /// <summary>
        /// Gets the instance of the dispatcher
        /// </summary>
        public static UnityMainThreadDispatcher Instance()
        {
            if (_instance == null)
            {
                var go = new GameObject("UnityMainThreadDispatcher");
                _instance = go.AddComponent<UnityMainThreadDispatcher>();
                DontDestroyOnLoad(go);
            }
            
            return _instance;
        }
        
        /// <summary>
        /// Enqueues an action to be executed on the main thread
        /// </summary>
        public void Enqueue(System.Action action)
        {
            lock (_lockObject)
            {
                _actions.Enqueue(action);
            }
        }
        
        private void Update()
        {
            lock (_lockObject)
            {
                while (_actions.Count > 0)
                {
                    _actions.Dequeue()?.Invoke();
                }
            }
        }
    }
}
