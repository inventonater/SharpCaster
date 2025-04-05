using System;

namespace Inventonater.Chromecast.Interfaces
{
    /// <summary>
    /// Interface for heartbeat channel
    /// </summary>
    public interface IHeartbeatChannel : IChromecastChannel
    {
        /// <summary>
        /// Raised when the status changes
        /// </summary>
        event EventHandler StatusChanged;
        
        /// <summary>
        /// Starts the timeout timer
        /// </summary>
        void StartTimeoutTimer();
        
        /// <summary>
        /// Stops the timeout timer
        /// </summary>
        void StopTimeoutTimer();
    }
}
