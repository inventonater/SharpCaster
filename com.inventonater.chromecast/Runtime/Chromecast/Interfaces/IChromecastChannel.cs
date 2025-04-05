using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;

namespace Inventonater.Chromecast.Interfaces
{
    /// <summary>
    /// Interface for Chromecast channels
    /// </summary>
    public interface IChromecastChannel
    {
        /// <summary>
        /// Gets or sets the client
        /// </summary>
        IChromecastClient Client { get; set; }
        
        /// <summary>
        /// Gets the namespace
        /// </summary>
        string Namespace { get; }
        
        /// <summary>
        /// Gets the logger
        /// </summary>
        ILogger Logger { get; }
        
        /// <summary>
        /// Called when a message is received
        /// </summary>
        /// <param name="message">The message</param>
        UniTask OnMessageReceivedAsync(IMessage message);
    }
}
