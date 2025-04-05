using Inventonater.Chromecast.Messages;
using Newtonsoft.Json;

namespace Inventonater.Chromecast.Messages.Receiver
{
    /// <summary>
    /// Message to launch an application on the Chromecast device
    /// </summary>
    public class LaunchMessage : MessageWithID
    {
        /// <summary>
        /// Gets the message type
        /// </summary>
        public override string Type => "LAUNCH";
        
        /// <summary>
        /// Gets or sets the application ID
        /// </summary>
        [JsonProperty("appId")]
        public string AppId { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the LaunchMessage class
        /// </summary>
        public LaunchMessage()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the LaunchMessage class with an application ID
        /// </summary>
        /// <param name="appId">The application ID</param>
        public LaunchMessage(string appId)
        {
            AppId = appId;
        }
    }
}
