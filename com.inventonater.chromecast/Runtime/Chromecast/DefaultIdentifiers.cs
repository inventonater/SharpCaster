using System;

namespace Inventonater.Chromecast
{
    /// <summary>
    /// Default identifiers for Chromecast communication
    /// </summary>
    public static class DefaultIdentifiers
    {
        /// <summary>
        /// The default sender ID
        /// </summary>
        public const string SENDER_ID = "sender-0";
        
        /// <summary>
        /// The default receiver ID
        /// </summary>
        public const string RECEIVER_ID = "receiver-0";
        
        /// <summary>
        /// The default destination ID
        /// </summary>
        public const string DESTINATION_ID = RECEIVER_ID;
    }
}
