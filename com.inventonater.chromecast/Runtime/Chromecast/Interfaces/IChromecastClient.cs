using Cysharp.Threading.Tasks;
using Inventonater.Chromecast.Channels;
using Inventonater.Chromecast.Interfaces;
using Inventonater.Chromecast.Messages;
using Inventonater.Chromecast.Models;
using Inventonater.Chromecast.Models.ChromecastStatus;
using Inventonater.Chromecast.Models.Media;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Inventonater.Chromecast.Interfaces
{
    /// <summary>
    /// Interface for Chromecast client
    /// </summary>
    public interface IChromecastClient
    {
        /// <summary>
        /// Raised when the sender is disconnected
        /// </summary>
        event EventHandler Disconnected;
        
        /// <summary>
        /// Gets the sender ID
        /// </summary>
        Guid SenderId { get; }
        
        /// <summary>
        /// Gets or sets the friendly name
        /// </summary>
        string FriendlyName { get; set; }
        
        /// <summary>
        /// Gets the media channel
        /// </summary>
        IMediaChannel MediaChannel { get; }
        
        /// <summary>
        /// Gets the heartbeat channel
        /// </summary>
        IHeartbeatChannel HeartbeatChannel { get; }
        
        /// <summary>
        /// Gets the receiver channel
        /// </summary>
        IReceiverChannel ReceiverChannel { get; }
        
        /// <summary>
        /// Gets the connection channel
        /// </summary>
        IConnectionChannel ConnectionChannel { get; }
        
        /// <summary>
        /// Connects to a Chromecast device
        /// </summary>
        /// <param name="chromecastReceiver">The Chromecast receiver</param>
        /// <returns>The Chromecast status</returns>
        UniTask<ChromecastStatus> ConnectChromecast(ChromecastReceiver chromecastReceiver);
        
        /// <summary>
        /// Disconnects from the Chromecast device
        /// </summary>
        UniTask DisconnectAsync();
        
        /// <summary>
        /// Sends a message to the Chromecast device
        /// </summary>
        /// <param name="channelLogger">The channel logger</param>
        /// <param name="ns">The namespace</param>
        /// <param name="message">The message</param>
        /// <param name="destinationId">The destination ID</param>
        UniTask SendAsync(ILogger channelLogger, string ns, IMessage message, string destinationId);
        
        /// <summary>
        /// Sends a message to the Chromecast device and waits for a response
        /// </summary>
        /// <typeparam name="TResponse">The response type</typeparam>
        /// <param name="channelLogger">The channel logger</param>
        /// <param name="ns">The namespace</param>
        /// <param name="message">The message</param>
        /// <param name="destinationId">The destination ID</param>
        /// <returns>The response</returns>
        UniTask<TResponse> SendAsync<TResponse>(ILogger channelLogger, string ns, IMessageWithId message, string destinationId) where TResponse : IMessageWithId;
        
        /// <summary>
        /// Gets a channel
        /// </summary>
        /// <typeparam name="TChannel">The channel type</typeparam>
        /// <returns>The channel</returns>
        TChannel GetChannel<TChannel>() where TChannel : IChromecastChannel;
        
        /// <summary>
        /// Launches an application on the Chromecast device
        /// </summary>
        /// <param name="applicationId">The application ID</param>
        /// <param name="joinExistingApplicationSession">Whether to join an existing application session</param>
        /// <returns>The Chromecast status</returns>
        UniTask<ChromecastStatus> LaunchApplicationAsync(string applicationId, bool joinExistingApplicationSession = true);
        
        /// <summary>
        /// Gets the different statuses
        /// </summary>
        /// <returns>A dictionary of namespace/status</returns>
        IDictionary<string, object> GetStatuses();
        
        /// <summary>
        /// Gets the Chromecast status
        /// </summary>
        /// <returns>The Chromecast status</returns>
        ChromecastStatus GetChromecastStatus();
        
        /// <summary>
        /// Gets the media status
        /// </summary>
        /// <returns>The media status</returns>
        MediaStatus GetMediaStatus();
    }
}
