using System.Collections.Generic;
using Inventonater.Chromecast.Messages;
using Inventonater.Chromecast.Models;
using Inventonater.Chromecast.Models.ChromecastStatus;
using Newtonsoft.Json;

namespace Inventonater.Chromecast.Messages.Receiver
{
    /// <summary>
    /// Message containing the Chromecast device status
    /// </summary>
    [ReceptionMessage("RECEIVER_STATUS")]
    public class ReceiverStatusMessage : MessageWithID, IStatusMessage<ChromecastStatus>
    {
        /// <summary>
        /// Gets the message type
        /// </summary>
        public override string Type => "RECEIVER_STATUS";
        
        /// <summary>
        /// Gets or sets the status
        /// </summary>
        [JsonProperty("status")]
        public ReceiverStatus Status { get; set; }
        
        /// <summary>
        /// Gets the ChromecastStatus
        /// </summary>
        public ChromecastStatus GetStatus()
        {
            return Status?.ChromecastStatus;
        }
        
        /// <summary>
        /// Container for the receiver status
        /// </summary>
        public class ReceiverStatus
        {
            /// <summary>
            /// Gets or sets the applications
            /// </summary>
            [JsonProperty("applications")]
            public List<ChromecastApplication> Applications { get; set; }
            
            /// <summary>
            /// Gets or sets whether the result is an active input
            /// </summary>
            [JsonProperty("isActiveInput")]
            public bool IsActiveInput { get; set; }
            
            /// <summary>
            /// Gets or sets whether the result is stand by
            /// </summary>
            [JsonProperty("isStandBy")]
            public bool IsStandBy { get; set; }
            
            /// <summary>
            /// Gets or sets the volume
            /// </summary>
            [JsonProperty("volume")]
            public Volume Volume { get; set; }
            
            /// <summary>
            /// Gets the ChromecastStatus object
            /// </summary>
            [JsonIgnore]
            public ChromecastStatus ChromecastStatus => new ChromecastStatus
            {
                Applications = Applications,
                Volume = Volume,
                IsActiveInput = IsActiveInput,
                IsStandBy = IsStandBy
            };
        }
    }
}
