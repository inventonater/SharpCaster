using System.Collections.Generic;

namespace Inventonater.Chromecast.Models.Media
{
    /// <summary>
    /// Represents the status of media
    /// </summary>
    public class MediaStatus
    {
        /// <summary>
        /// Gets or sets the media session ID
        /// </summary>
        public int MediaSessionId { get; set; }
        
        /// <summary>
        /// Gets or sets the player state
        /// </summary>
        public PlayerStateType PlayerState { get; set; }
        
        /// <summary>
        /// Gets or sets the idle reason
        /// </summary>
        public string IdleReason { get; set; }
        
        /// <summary>
        /// Gets or sets the current time
        /// </summary>
        public double CurrentTime { get; set; }
        
        /// <summary>
        /// Gets or sets the supported media commands
        /// </summary>
        public long SupportedMediaCommands { get; set; }
        
        /// <summary>
        /// Gets or sets the volume
        /// </summary>
        public Volume Volume { get; set; }
        
        /// <summary>
        /// Gets or sets the playback rate
        /// </summary>
        public double PlaybackRate { get; set; }
        
        /// <summary>
        /// Gets or sets the media information
        /// </summary>
        public Media Media { get; set; }
        
        /// <summary>
        /// Gets or sets the custom data
        /// </summary>
        public object CustomData { get; set; }
        
        /// <summary>
        /// Gets or sets the repeat mode
        /// </summary>
        public RepeatModeType RepeatMode { get; set; }
        
        /// <summary>
        /// Gets or sets the queue items
        /// </summary>
        public IEnumerable<QueueItem> Items { get; set; }
    }
}
