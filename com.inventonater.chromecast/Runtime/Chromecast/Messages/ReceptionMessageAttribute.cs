using System;

namespace Inventonater.Chromecast.Messages
{
    /// <summary>
    /// Attribute for received messages
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class ReceptionMessageAttribute : Attribute
    {
        /// <summary>
        /// Gets the message type
        /// </summary>
        public string MessageType { get; }
        
        /// <summary>
        /// Initializes a new instance of the ReceptionMessageAttribute class
        /// </summary>
        public ReceptionMessageAttribute()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the ReceptionMessageAttribute class with a specific message type
        /// </summary>
        /// <param name="messageType">The message type</param>
        public ReceptionMessageAttribute(string messageType)
        {
            MessageType = messageType;
        }
    }
}
