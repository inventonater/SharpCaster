using System.Collections.Generic;

namespace Inventonater.Chromecast.Models.Media
{
    /// <summary>
    /// Represents a media item
    /// </summary>
    public class Media
    {
        /// <summary>
        /// Gets or sets the content ID
        /// </summary>
        public string ContentId { get; set; }
        
        /// <summary>
        /// Gets or sets the content type
        /// </summary>
        public string ContentType { get; set; }
        
        /// <summary>
        /// Gets or sets the stream type
        /// </summary>
        public StreamType StreamType { get; set; } = StreamType.Buffered;
        
        /// <summary>
        /// Gets or sets the duration
        /// </summary>
        public double? Duration { get; set; }
        
        /// <summary>
        /// Gets or sets the metadata
        /// </summary>
        public MediaMetadata Metadata { get; set; }
        
        /// <summary>
        /// Gets or sets the text track style
        /// </summary>
        public object TextTrackStyle { get; set; }
        
        /// <summary>
        /// Gets or sets the tracks
        /// </summary>
        public IEnumerable<object> Tracks { get; set; }
        
        /// <summary>
        /// Gets or sets the content URL
        /// </summary>
        public string ContentUrl
        {
            get => ContentId;
            set => ContentId = value;
        }
    }
}
