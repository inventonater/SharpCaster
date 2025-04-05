namespace Inventonater.Chromecast.Interfaces
{
    /// <summary>
    /// Interface for Chromecast messages with IDs
    /// </summary>
    public interface IMessageWithId : IMessage
    {
        /// <summary>
        /// Gets or sets the request ID
        /// </summary>
        int RequestId { get; set; }
        
        /// <summary>
        /// Gets a value indicating whether the message has a request ID
        /// </summary>
        bool HasRequestId { get; }
    }
}
