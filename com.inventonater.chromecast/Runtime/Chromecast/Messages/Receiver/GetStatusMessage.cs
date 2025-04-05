using Inventonater.Chromecast.Messages;

namespace Inventonater.Chromecast.Messages.Receiver
{
    /// <summary>
    /// Message to get the Chromecast device status
    /// </summary>
    public class GetStatusMessage : MessageWithID
    {
        /// <summary>
        /// Gets the message type
        /// </summary>
        public override string Type => "GET_STATUS";
    }
}
