using System.Collections.Generic;

namespace Inventonater.Chromecast.Models.ChromecastStatus
{
    /// <summary>
    /// Represents a Chromecast application
    /// </summary>
    public class ChromecastApplication
    {
        /// <summary>
        /// Gets or sets the application ID
        /// </summary>
        public string AppId { get; set; }
        
        /// <summary>
        /// Gets or sets the display name
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Gets or sets the namespaces
        /// </summary>
        public IEnumerable<Namespace> Namespaces { get; set; }
        
        /// <summary>
        /// Gets or sets the session ID
        /// </summary>
        public string SessionId { get; set; }
        
        /// <summary>
        /// Gets or sets the status text
        /// </summary>
        public string StatusText { get; set; }
        
        /// <summary>
        /// Gets or sets the transport ID
        /// </summary>
        public string TransportId { get; set; }
    }
}
