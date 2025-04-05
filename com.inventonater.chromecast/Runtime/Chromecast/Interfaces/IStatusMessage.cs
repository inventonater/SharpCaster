namespace Inventonater.Chromecast.Interfaces
{
    /// <summary>
    /// Interface for messages that contain a status
    /// </summary>
    /// <typeparam name="TStatus">The status type</typeparam>
    public interface IStatusMessage<TStatus> where TStatus : class
    {
        /// <summary>
        /// Gets the status
        /// </summary>
        /// <returns>The status</returns>
        TStatus GetStatus();
    }
}
