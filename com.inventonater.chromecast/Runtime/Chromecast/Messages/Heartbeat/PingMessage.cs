using Inventonater.Chromecast.Messages;
using System.Runtime.Serialization;

namespace Inventonater.Chromecast.Messages.Heartbeat
{
    /// <summary>
    /// Ping message for the heartbeat channel
    /// </summary>
    [DataContract]
    [ReceptionMessage("PING")]
    public class PingMessage : Message
    {
        /// <summary>
        /// Gets the message type
        /// </summary>
        public override string Type => "PING";
    }
}
