using Inventonater.Chromecast.Messages;
using Newtonsoft.Json;

namespace Inventonater.Chromecast.Messages.Receiver
{
    /// <summary>
    /// Message to set the volume or mute state of the Chromecast device
    /// </summary>
    public class SetVolumeMessage : MessageWithID
    {
        /// <summary>
        /// Gets the message type
        /// </summary>
        public override string Type => "SET_VOLUME";
        
        /// <summary>
        /// Gets or sets the volume
        /// </summary>
        [JsonProperty("volume")]
        public VolumeObject Volume { get; set; }
        
        /// <summary>
        /// Gets or sets the volume level directly
        /// </summary>
        [JsonIgnore]
        public float Level
        {
            get => Volume?.Level ?? 0;
            set => Volume = new VolumeObject { Level = value };
        }
        
        /// <summary>
        /// Gets or sets the mute state directly
        /// </summary>
        [JsonIgnore]
        public bool Muted
        {
            get => Volume?.Muted ?? false;
            set
            {
                if (Volume == null)
                {
                    Volume = new VolumeObject();
                }
                
                Volume.Muted = value;
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the SetVolumeMessage class
        /// </summary>
        public SetVolumeMessage()
        {
            Volume = new VolumeObject();
        }
        
        /// <summary>
        /// Initializes a new instance of the SetVolumeMessage class with a specified volume level
        /// </summary>
        /// <param name="level">The volume level (0.0 - 1.0)</param>
        public SetVolumeMessage(float level) : this()
        {
            Level = level;
        }
        
        /// <summary>
        /// Container for volume information
        /// </summary>
        public class VolumeObject
        {
            /// <summary>
            /// Gets or sets the volume level
            /// </summary>
            [JsonProperty("level")]
            public float Level { get; set; }
            
            /// <summary>
            /// Gets or sets whether the device is muted
            /// </summary>
            [JsonProperty("muted")]
            public bool Muted { get; set; }
        }
    }
}
