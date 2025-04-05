using Cysharp.Threading.Tasks;
using Inventonater.Chromecast.Models.Media;
using System.Collections.Generic;

namespace Inventonater.Chromecast.Interfaces
{
    /// <summary>
    /// Interface for media channel
    /// </summary>
    public interface IMediaChannel : IStatusChannel<MediaStatus>
    {
        /// <summary>
        /// Gets the current media session ID
        /// </summary>
        int? CurrentMediaSessionId { get; }
        
        /// <summary>
        /// Gets the media status
        /// </summary>
        /// <returns>The media status</returns>
        UniTask<MediaStatus> GetMediaStatusAsync();
        
        /// <summary>
        /// Loads media
        /// </summary>
        /// <param name="media">The media to load</param>
        /// <param name="autoplay">Whether to autoplay the media</param>
        /// <param name="currentTime">The current time</param>
        /// <param name="customData">Custom data</param>
        /// <returns>The media status</returns>
        UniTask<MediaStatus> LoadAsync(Media media, bool autoplay = true, double currentTime = 0, object customData = null);
        
        /// <summary>
        /// Plays media
        /// </summary>
        /// <returns>The media status</returns>
        UniTask<MediaStatus> PlayAsync();
        
        /// <summary>
        /// Pauses media
        /// </summary>
        /// <returns>The media status</returns>
        UniTask<MediaStatus> PauseAsync();
        
        /// <summary>
        /// Stops media
        /// </summary>
        /// <returns>The media status</returns>
        UniTask<MediaStatus> StopAsync();
        
        /// <summary>
        /// Seeks media
        /// </summary>
        /// <param name="seconds">The seconds to seek to</param>
        /// <returns>The media status</returns>
        UniTask<MediaStatus> SeekAsync(double seconds);
        
        /// <summary>
        /// Sets the volume
        /// </summary>
        /// <param name="level">The volume level</param>
        /// <returns>The media status</returns>
        UniTask<MediaStatus> SetVolumeAsync(float level);
        
        /// <summary>
        /// Sets the mute state
        /// </summary>
        /// <param name="muted">The muted state</param>
        /// <returns>The media status</returns>
        UniTask<MediaStatus> SetMuteAsync(bool muted);
        
        /// <summary>
        /// Loads a queue of media items
        /// </summary>
        /// <param name="items">The queue items</param>
        /// <param name="startIndex">The start index</param>
        /// <param name="repeatMode">The repeat mode</param>
        /// <returns>The media status</returns>
        UniTask<MediaStatus> QueueLoadAsync(IEnumerable<QueueItem> items, int startIndex = 0, RepeatModeType repeatMode = RepeatModeType.RepeatOff);
        
        /// <summary>
        /// Gets the next item in the queue
        /// </summary>
        /// <returns>The media status</returns>
        UniTask<MediaStatus> QueueNextAsync();
        
        /// <summary>
        /// Gets the previous item in the queue
        /// </summary>
        /// <returns>The media status</returns>
        UniTask<MediaStatus> QueuePrevAsync();
    }
}
