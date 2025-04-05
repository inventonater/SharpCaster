using Cysharp.Threading.Tasks;
using Inventonater.Chromecast.Interfaces;
using Inventonater.Chromecast.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Inventonater.Chromecast.Unity
{
    /// <summary>
    /// Unity implementation of Chromecast device discovery
    /// </summary>
    public class UnityChromecastLocator : IChromecastLocator
    {
        /// <summary>
        /// Configuration for device discovery
        /// </summary>
        public class DiscoveryConfig
        {
            /// <summary>
            /// The port used by Chromecast devices (default: 8009)
            /// </summary>
            public int ChromecastPort { get; set; } = 8009;
            
            /// <summary>
            /// Custom IP addresses to scan (in addition to local subnet)
            /// </summary>
            public List<string> CustomIpAddresses { get; set; } = new List<string>();
            
            /// <summary>
            /// Timeout in milliseconds for each connection attempt
            /// </summary>
            public int ConnectionTimeoutMs { get; set; } = 300;
            
            /// <summary>
            /// Whether to scan the local subnet
            /// </summary>
            public bool ScanLocalSubnet { get; set; } = true;
            
            /// <summary>
            /// Maximum concurrent connection attempts
            /// </summary>
            public int MaxConcurrentScans { get; set; } = 10;
        }
        
        private const int CHROMECAST_PORT = 8009;
        private readonly List<ChromecastReceiver> _knownDevices = new List<ChromecastReceiver>();
        private DiscoveryConfig _config;
        
        /// <summary>
        /// Initializes a new instance of the UnityChromecastLocator class
        /// </summary>
        /// <param name="config">Optional discovery configuration</param>
        public UnityChromecastLocator(DiscoveryConfig config = null)
        {
            _config = config ?? new DiscoveryConfig();
            
            // Add simulated devices for testing in editor
            if (Application.isEditor)
            {
                AddDevice("Living Room TV", "192.168.1.100");
                AddDevice("Bedroom TV", "192.168.1.101");
            }
        }
        
        /// <summary>
        /// Adds a known device
        /// </summary>
        /// <param name="name">The device name</param>
        /// <param name="ipAddress">The IP address</param>
        /// <param name="port">The port (default: 8009)</param>
        /// <returns>The added device</returns>
        public ChromecastReceiver AddDevice(string name, string ipAddress, int port = CHROMECAST_PORT)
        {
            var uri = new Uri($"https://{ipAddress}:{port}");
            var device = new ChromecastReceiver
            {
                Name = name,
                DeviceUri = uri,
                Port = port
            };
            
            // Check if device already exists by IP
            var existingDevice = _knownDevices.FirstOrDefault(d => 
                d.DeviceUri.Host.Equals(ipAddress, StringComparison.OrdinalIgnoreCase));
                
            if (existingDevice != null)
            {
                // Remove existing device with same IP
                _knownDevices.Remove(existingDevice);
            }
            
            _knownDevices.Add(device);
            return device;
        }
        
        /// <summary>
        /// Gets all known devices
        /// </summary>
        public IEnumerable<ChromecastReceiver> GetKnownDevices()
        {
            return _knownDevices.ToList();
        }
        
        /// <summary>
        /// Clears all known devices
        /// </summary>
        public void ClearKnownDevices()
        {
            _knownDevices.Clear();
        }
        
        /// <summary>
        /// Finds Chromecast devices on the network
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The discovered Chromecast devices</returns>
        public async UniTask<IEnumerable<ChromecastReceiver>> FindReceiversAsync(CancellationToken cancellationToken = default)
        {
            // In the editor, return simulated devices for testing
            if (Application.isEditor)
            {
                Debug.Log("Using simulated Chromecast devices in editor");
                return _knownDevices;
            }
            
            var discoveredDevices = new List<ChromecastReceiver>();
            
            // First, add all known devices
            discoveredDevices.AddRange(_knownDevices);
            
            if (_config.CustomIpAddresses?.Count > 0)
            {
                Debug.Log($"Scanning {_config.CustomIpAddresses.Count} custom IP addresses...");
                
                try
                {
                    var tasks = new List<UniTask>();
                    foreach (var ip in _config.CustomIpAddresses)
                    {
                        tasks.Add(ScanIpAddressAsync(ip, discoveredDevices, cancellationToken));
                    }
                    
                    // Process custom IPs in batches to avoid too many concurrent connections
                    foreach (var batch in tasks.Batch(_config.MaxConcurrentScans))
                    {
                        await UniTask.WhenAll(batch);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error scanning custom IPs: {ex.Message}");
                }
            }
            
            if (_config.ScanLocalSubnet)
            {
                Debug.Log("Scanning local subnet...");
                try
                {
                    // Get the local subnet for scanning
                    var localIps = GetLocalNetworkAddresses();
                    if (localIps.Count > 0)
                    {
                        foreach (var subnet in localIps)
                        {
                            Debug.Log($"Scanning subnet: {subnet.Item1}/24");
                            
                            // Create scan tasks for each IP in the subnet
                            var tasks = new List<UniTask>();
                            for (int i = 1; i <= 254; i++)
                            {
                                string ip = $"{subnet.Item1}.{i}";
                                
                                // Skip if this is our own IP
                                if (ip == subnet.Item2)
                                    continue;
                                    
                                tasks.Add(ScanIpAddressAsync(ip, discoveredDevices, cancellationToken));
                            }
                            
                            // Process in batches
                            foreach (var batch in tasks.Batch(_config.MaxConcurrentScans))
                            {
                                await UniTask.WhenAll(batch);
                                
                                // Check if canceled
                                if (cancellationToken.IsCancellationRequested)
                                    break;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("No local network interfaces found. Falling back to common IP ranges.");
                        
                        // Fallback to scanning common ranges
                        var tasks = new List<UniTask>
                        {
                            ScanIpAddressAsync("192.168.1.100", discoveredDevices, cancellationToken),
                            ScanIpAddressAsync("192.168.1.101", discoveredDevices, cancellationToken),
                            ScanIpAddressAsync("192.168.0.100", discoveredDevices, cancellationToken),
                            ScanIpAddressAsync("192.168.0.101", discoveredDevices, cancellationToken)
                        };
                        
                        await UniTask.WhenAll(tasks);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error scanning local subnet: {ex.Message}");
                }
            }
            
            Debug.Log($"Found {discoveredDevices.Count} total Chromecast devices");
            
            // Save any new devices to the known devices list
            foreach (var device in discoveredDevices)
            {
                if (!_knownDevices.Any(d => d.DeviceUri.Host == device.DeviceUri.Host))
                {
                    _knownDevices.Add(device);
                }
            }
            
            return discoveredDevices;
        }
        
        private async UniTask ScanIpAddressAsync(string ipAddress, List<ChromecastReceiver> devices, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;
                
            try
            {
                // Create TCP client with short timeout
                using (var client = new TcpClient())
                {
                    // Use a short timeout to quickly skip non-responsive IPs
                    var connectTask = UniTask.RunOnThreadPool(() => 
                        client.ConnectAsync(ipAddress, _config.ChromecastPort));
                    
                    // Wait for connection with timeout
                    var timeoutTask = UniTask.Delay(_config.ConnectionTimeoutMs);
                    
                    // If we can connect to the port, it might be a Chromecast
                    if (await UniTask.WhenAny(connectTask, timeoutTask) == 0)
                    {
                        Uri deviceUri = new Uri($"https://{ipAddress}:{_config.ChromecastPort}");
                        
                        // Get device name (would query device info API in a real implementation)
                        string name = await GetDeviceNameAsync(client, $"Chromecast ({ipAddress})");
                        
                        var newDevice = new ChromecastReceiver {
                            Name = name,
                            DeviceUri = deviceUri,
                            Port = _config.ChromecastPort
                        };
                        
                        // Add to devices list if not already present
                        bool alreadyExists = devices.Any(d => d.DeviceUri.Host == ipAddress);
                        if (!alreadyExists)
                        {
                            lock (devices)
                            {
                                devices.Add(newDevice);
                            }
                            Debug.Log($"Found Chromecast device: {name} at {ipAddress}:{_config.ChromecastPort}");
                        }
                    }
                }
            }
            catch
            {
                // Just swallow exceptions - this is an opportunistic scan
            }
        }
        
        private List<Tuple<string, string>> GetLocalNetworkAddresses()
        {
            var result = new List<Tuple<string, string>>();
            
            try
            {
                // Get all network interfaces
                var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(i => i.OperationalStatus == OperationalStatus.Up && 
                           (i.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || 
                            i.NetworkInterfaceType == NetworkInterfaceType.Ethernet))
                    .ToList();
                
                foreach (var adapter in interfaces)
                {
                    var props = adapter.GetIPProperties();
                    
                    // Get IPv4 addresses
                    foreach (var addr in props.UnicastAddresses)
                    {
                        if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            string ipAddress = addr.Address.ToString();
                            // Get subnet part (first 3 octets)
                            string[] octets = ipAddress.Split('.');
                            if (octets.Length == 4)
                            {
                                string subnet = $"{octets[0]}.{octets[1]}.{octets[2]}";
                                result.Add(new Tuple<string, string>(subnet, ipAddress));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting network interfaces: {ex.Message}");
            }
            
            return result;
        }
        
        // Helper extension method to batch tasks
        private static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
            var batch = new List<T>(batchSize);
            foreach (var item in source)
            {
                batch.Add(item);
                if (batch.Count == batchSize)
                {
                    yield return batch;
                    batch = new List<T>(batchSize);
                }
            }
            
            if (batch.Count > 0)
            {
                yield return batch;
            }
        }
        
        private async UniTask<string> GetDeviceNameAsync(TcpClient client, string defaultName)
        {
            // In a real implementation, you would query the device for its friendly name
            // This is a placeholder - would actually query the device info API

            await UniTask.CompletedTask; // Just to make it async
            return defaultName;
        }
    }
}
