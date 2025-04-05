namespace Inventonater.Chromecast.Interfaces
{
    /// <summary>
    /// Interface for status channel
    /// </summary>
    /// <typeparam name="TStatus">The status type</typeparam>
    public interface IStatusChannel<TStatus> : IChromecastChannel where TStatus : class
    {
        /// <summary>
        /// Gets or sets the status
        /// </summary>
        TStatus Status { get; set; }
    }
}
