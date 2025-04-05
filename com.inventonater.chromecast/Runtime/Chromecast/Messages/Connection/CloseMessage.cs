using Inventonater.Chromecast.Messages;

namespace Inventonater.Chromecast.Messages.Connection
{
    /// <summary>
    /// Message to close the connection to the Chromecast device
    /// </summary>
    [ReceptionMessage("CLOSE")]
    public class CloseMessage : Message
    {
        /// <summary>
        /// Gets the message type
        /// </summary>
        public override string Type => "CLOSE";
    }
}
