using Inventonater.Chromecast.Messages;
using System.Runtime.Serialization;

namespace Inventonater.Chromecast.Messages.Heartbeat
{
    /// <summary>
    /// Pong message for the heartbeat channel
    /// </summary>
    [DataContract]
    public class PongMessage : Message
    {
        /// <summary>
        /// Gets the message type
        /// </summary>
        public override string Type => "PONG";
    }
}
