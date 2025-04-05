namespace Inventonater.Chromecast.Models
{
    /// <summary>
    /// Represents a volume
    /// </summary>
    public class Volume
    {
        /// <summary>
        /// Gets or sets the level
        /// </summary>
        public float Level { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the volume is muted
        /// </summary>
        public bool Muted { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether volume is controlled by the cast device
        /// </summary>
        public bool ControlType { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating maximum volume level
        /// </summary>
        public float MaximumLevel { get; set; } = 1.0f;
        
        /// <summary>
        /// Gets or sets a value indicating volume step
        /// </summary>
        public float Step { get; set; } = 0.05f;
    }
}
