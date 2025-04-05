using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Inventonater.Chromecast.Interfaces;
using Inventonater.Chromecast.Messages.Heartbeat;
using System;
using System.Timers;
using UnityEngine;

namespace Inventonater.Chromecast.Channels
{
    /// <summary>
    /// Heartbeat channel. Responds to ping messages with pong message
    /// </summary>
    public class HeartbeatChannel : ChromecastChannel, IHeartbeatChannel
    {
        private readonly Timer _timer;

        /// <summary>
        /// Event raised when the heartbeat status changes (timeout)
        /// </summary>
        public event EventHandler StatusChanged;

        /// <summary>
        /// Initializes a new instance of HeartbeatChannel class
        /// </summary>
        /// <param name="logger">Optional logger</param>
        public HeartbeatChannel(ILogger<HeartbeatChannel> logger = null) : base("tp.heartbeat", logger)
        {
            // Set timeout to 10 seconds because Chromecast only waits for 8 seconds for response
            _timer = new Timer(10000);
            _timer.Elapsed += TimerElapsed;
            _timer.AutoReset = false;
        }

        /// <summary>
        /// Called when a message for this channel is received
        /// </summary>
        /// <param name="message">message to process</param>
        public override async UniTask OnMessageReceivedAsync(IMessage message)
        {
            if (message is PingMessage)
            {
                _timer.Stop();
                await SendAsync(new PongMessage());
                _timer.Start();
                
                Logger?.LogDebug("Pong sent - Heartbeat Timer restarted.");
                Debug.Log("[HeartbeatChannel] Pong sent - Heartbeat Timer restarted.");
            }
        }

        /// <summary>
        /// Starts the timeout timer
        /// </summary>
        public void StartTimeoutTimer()
        {
            _timer.Start();
            Logger?.LogTrace("Started heartbeat timeout timer");
            Debug.Log("[HeartbeatChannel] Started heartbeat timeout timer");
        }

        /// <summary>
        /// Stops the timeout timer
        /// </summary>
        public void StopTimeoutTimer()
        {
            _timer.Stop();
            Logger?.LogTrace("Stopped heartbeat timeout timer");
            Debug.Log("[HeartbeatChannel] Stopped heartbeat timeout timer");
        }

        /// <summary>
        /// Called when the timer elapses (timeout)
        /// </summary>
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            Logger?.LogInformation("Heartbeat timeout");
            Debug.LogWarning("[HeartbeatChannel] Heartbeat timeout");
            
            // Use Unity's main thread to invoke the event
            UniTask.RunOnUiThread(() => 
            {
                StatusChanged?.Invoke(this, e);
            }).Forget();
        }
    }
}
