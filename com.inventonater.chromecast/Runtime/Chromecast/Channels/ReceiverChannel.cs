using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Inventonater.Chromecast.Interfaces;
using Inventonater.Chromecast.Messages.Receiver;
using Inventonater.Chromecast.Models;
using Inventonater.Chromecast.Models.ChromecastStatus;

namespace Inventonater.Chromecast.Channels
{
    /// <summary>
    /// Receiver channel, responsible for handling Chromecast device status and application launching
    /// </summary>
    public class ReceiverChannel : StatusChannel<ChromecastStatus>, IReceiverChannel
    {
        /// <summary>
        /// Initializes a new instance of the ReceiverChannel class
        /// </summary>
        /// <param name="logger">The logger</param>
        public ReceiverChannel(ILogger<ReceiverChannel> logger = null) 
            : base("receiver", logger)
        {
        }

        /// <summary>
        /// Gets the Chromecast status
        /// </summary>
        /// <returns>The Chromecast status</returns>
        public async UniTask<ChromecastStatus> GetChromecastStatusAsync()
        {
            var response = await SendAsync<ReceiverStatusMessage>(new GetStatusMessage());
            Status = response.GetStatus();
            return Status;
        }
        
        /// <summary>
        /// Launches an application on the Chromecast device
        /// </summary>
        /// <param name="appId">The application ID</param>
        /// <returns>The updated Chromecast status</returns>
        public async UniTask<ReceiverStatusMessage> LaunchApplicationAsync(string appId)
        {
            Logger?.LogInformation($"Launching app: {appId}");
            var response = await SendAsync<ReceiverStatusMessage>(new LaunchMessage(appId));
            Status = response.GetStatus();
            return response;
        }
        
        /// <summary>
        /// Sets the volume level
        /// </summary>
        /// <param name="level">The volume level (0.0 - 1.0)</param>
        /// <returns>The updated Chromecast status</returns>
        public async UniTask<ChromecastStatus> SetVolumeAsync(float level)
        {
            Logger?.LogInformation($"Setting volume: {level}");
            var response = await SendAsync<ReceiverStatusMessage>(new SetVolumeMessage(level));
            Status = response.GetStatus();
            return Status;
        }
        
        /// <summary>
        /// Sets the mute state
        /// </summary>
        /// <param name="muted">Whether to mute the device</param>
        /// <returns>The updated Chromecast status</returns>
        public async UniTask<ChromecastStatus> SetMuteAsync(bool muted)
        {
            Logger?.LogInformation($"Setting mute: {muted}");
            var response = await SendAsync<ReceiverStatusMessage>(new SetVolumeMessage { Muted = muted });
            Status = response.GetStatus();
            return Status;
        }
        
        /// <summary>
        /// Called when a message for this channel is received
        /// </summary>
        /// <param name="message">Message to process</param>
        public override async UniTask OnMessageReceivedAsync(IMessage message)
        {
            if (message is ReceiverStatusMessage statusMessage)
            {
                Status = statusMessage.GetStatus();
            }
            
            await base.OnMessageReceivedAsync(message);
        }
    }
}
