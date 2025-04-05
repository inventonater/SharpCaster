using Cysharp.Threading.Tasks;
using Inventonater.Chromecast.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Inventonater.Chromecast.Unity.Examples
{
    /// <summary>
    /// Example usage of ChromecastManager in a Unity application
    /// </summary>
    public class ChromecastExample : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ChromecastManager _chromecastManager;
        [SerializeField] private Dropdown _deviceDropdown;
        [SerializeField] private Button _scanButton;
        [SerializeField] private Button _connectButton;
        [SerializeField] private Button _disconnectButton;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _stopButton;
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private InputField _urlInput;
        
        [Header("Configuration")]
        [SerializeField] private string _defaultAppId = "CC1AD845"; // Default Media Receiver
        [SerializeField] private string _defaultMediaUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/CastVideos/mp4/BigBuckBunny.mp4";
        
        private List<ChromecastReceiver> _devices = new List<ChromecastReceiver>();
        
        private void Start()
        {
            if (_chromecastManager == null)
            {
                _chromecastManager = FindObjectOfType<ChromecastManager>();
                
                if (_chromecastManager == null)
                {
                    _chromecastManager = gameObject.AddComponent<ChromecastManager>();
                }
            }
            
            SetupUIEvents();
            UpdateUIState(false);
            
            // Set default media URL
            if (_urlInput != null)
            {
                _urlInput.text = _defaultMediaUrl;
            }
            
            // Auto-scan for devices on start
            ScanForDevices().Forget();
        }
        
        private void SetupUIEvents()
        {
            if (_scanButton != null)
                _scanButton.onClick.AddListener(() => ScanForDevices().Forget());
            
            if (_connectButton != null)
                _connectButton.onClick.AddListener(() => ConnectToSelectedDevice().Forget());
            
            if (_disconnectButton != null)
                _disconnectButton.onClick.AddListener(() => DisconnectFromDevice().Forget());
            
            if (_playButton != null)
                _playButton.onClick.AddListener(() => PlayMedia().Forget());
            
            if (_pauseButton != null)
                _pauseButton.onClick.AddListener(() => PauseMedia().Forget());
            
            if (_stopButton != null)
                _stopButton.onClick.AddListener(() => StopMedia().Forget());
            
            if (_volumeSlider != null)
                _volumeSlider.onValueChanged.AddListener(value => SetVolume(value).Forget());
            
            // Subscribe to ChromecastManager events
            _chromecastManager.OnConnected += connected => UpdateUIState(connected);
            _chromecastManager.OnDisconnected += () => UpdateUIState(false);
        }
        
        private async UniTaskVoid ScanForDevices()
        {
            if (_scanButton != null)
                _scanButton.interactable = false;
            
            Debug.Log("Scanning for Chromecast devices...");
            
            _devices = (await _chromecastManager.DiscoverDevicesAsync()).ToList();
            
            if (_deviceDropdown != null)
            {
                _deviceDropdown.ClearOptions();
                
                if (_devices.Count > 0)
                {
                    var options = _devices.Select(d => new Dropdown.OptionData(d.Name)).ToList();
                    _deviceDropdown.AddOptions(options);
                    _deviceDropdown.value = 0;
                    _connectButton.interactable = true;
                }
                else
                {
                    _deviceDropdown.AddOptions(new List<string> { "No devices found" });
                    _connectButton.interactable = false;
                }
            }
            
            Debug.Log($"Found {_devices.Count} Chromecast devices");
            
            if (_scanButton != null)
                _scanButton.interactable = true;
        }
        
        private async UniTaskVoid ConnectToSelectedDevice()
        {
            if (_devices.Count == 0 || _deviceDropdown.value >= _devices.Count)
                return;
            
            var device = _devices[_deviceDropdown.value];
            
            Debug.Log($"Connecting to {device.Name}...");
            await _chromecastManager.ConnectToDeviceAsync(device);
            
            // Launch the default media receiver app
            if (_chromecastManager.IsConnected)
            {
                await _chromecastManager.LaunchApplicationAsync(_defaultAppId);
            }
        }
        
        private async UniTaskVoid DisconnectFromDevice()
        {
            Debug.Log("Disconnecting from device...");
            await _chromecastManager.DisconnectAsync();
        }
        
        private async UniTaskVoid PlayMedia()
        {
            if (!_chromecastManager.IsConnected)
                return;
            
            // If we have a URL in the input field, load it first
            string url = _urlInput?.text ?? _defaultMediaUrl;
            
            if (!string.IsNullOrEmpty(url) && _chromecastManager.MediaStatus == null)
            {
                Debug.Log($"Loading media: {url}");
                await _chromecastManager.LoadMediaAsync(url);
            }
            else
            {
                Debug.Log("Playing media");
                await _chromecastManager.PlayAsync();
            }
        }
        
        private async UniTaskVoid PauseMedia()
        {
            if (!_chromecastManager.IsConnected)
                return;
            
            Debug.Log("Pausing media");
            await _chromecastManager.PauseAsync();
        }
        
        private async UniTaskVoid StopMedia()
        {
            if (!_chromecastManager.IsConnected)
                return;
            
            Debug.Log("Stopping media");
            await _chromecastManager.StopAsync();
        }
        
        private async UniTaskVoid SetVolume(float value)
        {
            if (!_chromecastManager.IsConnected)
                return;
            
            await _chromecastManager.SetVolumeAsync(value);
        }
        
        private void UpdateUIState(bool connected)
        {
            if (_connectButton != null)
                _connectButton.interactable = !connected && _devices.Count > 0;
            
            if (_disconnectButton != null)
                _disconnectButton.interactable = connected;
            
            if (_playButton != null)
                _playButton.interactable = connected;
            
            if (_pauseButton != null)
                _pauseButton.interactable = connected;
            
            if (_stopButton != null)
                _stopButton.interactable = connected;
            
            if (_volumeSlider != null)
                _volumeSlider.interactable = connected;
            
            if (_urlInput != null)
                _urlInput.interactable = connected;
        }
    }
}
