using System;
using System.Collections.Generic;

namespace Inventonater.Chromecast.Models
{
    /// <summary>
    /// Represents a Chromecast device on the network
    /// </summary>
    public class ChromecastReceiver
    {
        /// <summary>
        /// Gets or sets the device URI
        /// </summary>
        public Uri DeviceUri { get; set; }
        
        /// <summary>
        /// Gets or sets the device name
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the device model
        /// </summary>
        public string Model { get; set; }
        
        /// <summary>
        /// Gets or sets the device version
        /// </summary>
        public string Version { get; set; }
        
        /// <summary>
        /// Gets or sets the device status
        /// </summary>
        public string Status { get; set; }
        
        /// <summary>
        /// Gets or sets the device port
        /// </summary>
        public int Port { get; set; }
        
        /// <summary>
        /// Gets or sets additional device information
        /// </summary>
        public Dictionary<string, string> ExtraInformation { get; set; }
    }
}
