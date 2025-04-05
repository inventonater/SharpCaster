using Cysharp.Threading.Tasks;
using Inventonater.Chromecast.Models;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Inventonater.Chromecast.Interfaces
{
    /// <summary>
    /// Interface for finding Chromecast devices on the network
    /// </summary>
    public interface IChromecastLocator
    {
        /// <summary>
        /// Event raised when a Chromecast device is found
        /// </summary>
        event EventHandler<ChromecastReceiver> ChromecastReceivedFound;
        
        /// <summary>
        /// Find the available Chromecast receivers
        /// </summary>
        /// <returns>A collection of Chromecast receivers</returns>
        UniTask<IEnumerable<ChromecastReceiver>> FindReceiversAsync();
        
        /// <summary>
        /// Find the available Chromecast receivers
        /// </summary>
        /// <param name="cancellationToken">Enable to cancel the operation</param>
        /// <returns>A collection of Chromecast receivers</returns>
        UniTask<IEnumerable<ChromecastReceiver>> FindReceiversAsync(CancellationToken cancellationToken);
    }
}
