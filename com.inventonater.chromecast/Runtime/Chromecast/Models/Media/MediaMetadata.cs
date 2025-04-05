using System;
using System.Collections.Generic;

namespace Inventonater.Chromecast.Models.Media
{
    /// <summary>
    /// Represents media metadata
    /// </summary>
    public class MediaMetadata
    {
        /// <summary>
        /// Gets or sets the metadata type
        /// </summary>
        public MetadataType MetadataType { get; set; }
        
        /// <summary>
        /// Gets or sets the title
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Gets or sets the subtitle
        /// </summary>
        public string Subtitle { get; set; }
        
        /// <summary>
        /// Gets or sets the artist
        /// </summary>
        public string Artist { get; set; }
        
        /// <summary>
        /// Gets or sets the album name
        /// </summary>
        public string AlbumName { get; set; }
        
        /// <summary>
        /// Gets or sets the album artist
        /// </summary>
        public string AlbumArtist { get; set; }
        
        /// <summary>
        /// Gets or sets the release date
        /// </summary>
        public DateTime? ReleaseDate { get; set; }
        
        /// <summary>
        /// Gets or sets the images
        /// </summary>
        public IEnumerable<Image> Images { get; set; }
    }
}
