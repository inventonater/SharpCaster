using System.Collections.Generic;

namespace Inventonater.Chromecast.Models.ChromecastStatus
{
    /// <summary>
    /// Represents the status of a Chromecast device
    /// </summary>
    public class ChromecastStatus
    {
        /// <summary>
        /// Gets or sets a value indicating whether the device is active
        /// </summary>
        public bool IsActiveInput { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the device is stand by
        /// </summary>
        public bool IsStandBy { get; set; }
        
        /// <summary>
        /// Gets or sets the volume
        /// </summary>
        public Volume Volume { get; set; }
        
        /// <summary>
        /// Gets or sets the applications
        /// </summary>
        public IEnumerable<ChromecastApplication> Applications { get; set; }
    }
}
