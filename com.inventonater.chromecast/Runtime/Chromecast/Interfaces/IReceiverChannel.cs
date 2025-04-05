using Cysharp.Threading.Tasks;
using Inventonater.Chromecast.Models.ChromecastStatus;

namespace Inventonater.Chromecast.Interfaces
{
    /// <summary>
    /// Interface for receiver channel
    /// </summary>
    public interface IReceiverChannel : IStatusChannel<ChromecastStatus>
    {
        /// <summary>
        /// Gets the Chromecast status
        /// </summary>
        /// <returns>The Chromecast status</returns>
        UniTask<ChromecastStatus> GetChromecastStatusAsync();
        
        /// <summary>
        /// Launches an application
        /// </summary>
        /// <param name="applicationId">The application ID</param>
        /// <returns>The receiver status</returns>
        UniTask<ReceiverStatusMessage> LaunchApplicationAsync(string applicationId);
        
        /// <summary>
        /// Stops the application
        /// </summary>
        /// <param name="sessionId">The session ID</param>
        /// <returns>The response</returns>
        UniTask<IMessageWithId> StopApplicationAsync(string sessionId = null);
        
        /// <summary>
        /// Sets the volume
        /// </summary>
        /// <param name="level">The volume level</param>
        /// <returns>The receiver status</returns>
        UniTask<ReceiverStatusMessage> SetVolumeAsync(float level);
        
        /// <summary>
        /// Sets the mute state
        /// </summary>
        /// <param name="muted">The muted state</param>
        /// <returns>The receiver status</returns>
        UniTask<ReceiverStatusMessage> SetMuteAsync(bool muted);
    }
}
