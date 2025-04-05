namespace Inventonater.Chromecast.Models.Media
{
    /// <summary>
    /// Represents a queue item
    /// </summary>
    public class QueueItem
    {
        /// <summary>
        /// Gets or sets the item ID
        /// </summary>
        public int ItemId { get; set; }
        
        /// <summary>
        /// Gets or sets the media
        /// </summary>
        public Media Media { get; set; }
        
        /// <summary>
        /// Gets or sets the autoplay
        /// </summary>
        public bool Autoplay { get; set; }
        
        /// <summary>
        /// Gets or sets the start time
        /// </summary>
        public double StartTime { get; set; }
        
        /// <summary>
        /// Gets or sets the preload time
        /// </summary>
        public double PreloadTime { get; set; }
        
        /// <summary>
        /// Gets or sets the custom data
        /// </summary>
        public object CustomData { get; set; }
    }
}
