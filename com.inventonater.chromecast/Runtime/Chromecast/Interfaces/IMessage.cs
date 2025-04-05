namespace Inventonater.Chromecast.Interfaces
{
    /// <summary>
    /// Interface for Chromecast messages
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// Gets the message type
        /// </summary>
        string Type { get; }
    }
}
