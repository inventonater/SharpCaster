using Inventonater.Chromecast.Interfaces;

namespace Inventonater.Chromecast.Messages
{
    /// <summary>
    /// Base implementation of <see cref="IMessageWithId"/>
    /// </summary>
    public abstract class MessageWithID : Message, IMessageWithId
    {
        private static int _nextId = 1;
        
        /// <summary>
        /// Gets the next available ID
        /// </summary>
        private static int NextId => _nextId++;
        
        /// <summary>
        /// Gets or sets the request ID
        /// </summary>
        public int RequestId { get; set; }
        
        /// <summary>
        /// Gets a value indicating whether the message has a request ID
        /// </summary>
        public bool HasRequestId => RequestId > 0;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageWithID"/> class
        /// </summary>
        protected MessageWithID()
        {
            RequestId = NextId;
        }
    }
}
