using Inventonater.Chromecast.Messages;
using System;

namespace Inventonater.Chromecast.Messages.Connection
{
    /// <summary>
    /// Message to connect to the Chromecast device
    /// </summary>
    [ReceptionMessage("CONNECT")]
    public class ConnectMessage : Message
    {
        /// <summary>
        /// Gets the message type
        /// </summary>
        public override string Type => "CONNECT";
        
        /// <summary>
        /// Gets or sets the origin
        /// </summary>
        public Origin Origin { get; set; } = new Origin();
        
        /// <summary>
        /// Gets or sets the user agent
        /// </summary>
        public UserAgent UserAgent { get; set; } = new UserAgent();
        
        /// <summary>
        /// Platform details
        /// </summary>
        public class Origin
        {
            /// <summary>
            /// Gets or sets the platform
            /// </summary>
            public string Platform { get; set; } = "UNITY";
            
            /// <summary>
            /// Gets or sets the sender ID
            /// </summary>
            public Guid SenderPlatformId { get; set; } = Guid.NewGuid();
        }
        
        /// <summary>
        /// User agent details
        /// </summary>
        public class UserAgent
        {
            /// <summary>
            /// Gets or sets the user agent
            /// </summary>
            public string Value { get; set; } = "Inventonater.Chromecast";
        }
    }
}
