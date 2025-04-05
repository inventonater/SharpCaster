using Inventonater.Chromecast.Interfaces;

namespace Inventonater.Chromecast.Messages
{
    /// <summary>
    /// Base implementation of <see cref="IMessage"/>
    /// </summary>
    public abstract class Message : IMessage
    {
        /// <summary>
        /// Gets the message type
        /// </summary>
        public abstract string Type { get; }
    }
}
