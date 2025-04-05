namespace Inventonater.Chromecast.Models.Media
{
    /// <summary>
    /// Represents the type of media metadata
    /// </summary>
    public enum MetadataType
    {
        /// <summary>
        /// Generic metadata type
        /// </summary>
        Generic = 0,
        
        /// <summary>
        /// Movie metadata type
        /// </summary>
        Movie = 1,
        
        /// <summary>
        /// TV show metadata type
        /// </summary>
        TvShow = 2,
        
        /// <summary>
        /// Music track metadata type
        /// </summary>
        MusicTrack = 3,
        
        /// <summary>
        /// Photo metadata type
        /// </summary>
        Photo = 4
    }
}
