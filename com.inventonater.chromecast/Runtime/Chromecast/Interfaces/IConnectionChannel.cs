using Cysharp.Threading.Tasks;

namespace Inventonater.Chromecast.Interfaces
{
    /// <summary>
    /// Interface for connection channel
    /// </summary>
    public interface IConnectionChannel : IChromecastChannel
    {
        /// <summary>
        /// Connects to the receiver
        /// </summary>
        UniTask ConnectAsync();
        
        /// <summary>
        /// Connects to the receiver with a specific transport ID
        /// </summary>
        /// <param name="transportId">The transport ID</param>
        UniTask ConnectAsync(string transportId);
        
        /// <summary>
        /// Disconnects from the receiver
        /// </summary>
        UniTask DisconnectAsync();
    }
}
