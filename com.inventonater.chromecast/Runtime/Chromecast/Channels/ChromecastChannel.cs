using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Inventonater.Chromecast.Interfaces;
using System.Threading.Tasks;

namespace Inventonater.Chromecast.Channels
{
    /// <summary>
    /// Channel base class
    /// </summary>
    public abstract class ChromecastChannel : IChromecastChannel
    {
        private const string BASE_NAMESPACE = "urn:x-cast:com.google.cast";
        
        /// <summary>
        /// Gets the logger
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Initialization
        /// </summary>
        protected ChromecastChannel()
        {
        }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="ns">namespace</param>
        /// <param name="logger">The logger</param>
        /// <param name="useBaseNamespace">When true add urn:x-cast:com.google.cast to beginning of namespace</param>
        protected ChromecastChannel(string ns, ILogger logger, bool useBaseNamespace = true)
        {
            Logger = logger;
            if (useBaseNamespace)
            {
                Namespace = $"{BASE_NAMESPACE}.{ns}";
            }
            else
            {
                Namespace = ns;
            }
        }

        /// <summary>
        /// Gets or sets the client
        /// </summary>
        public virtual IChromecastClient Client { get; set; }

        /// <summary>
        /// Gets the full namespace
        /// </summary>
        public string Namespace { get; protected set; }

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="message">message to send</param>
        /// <param name="destinationId">destination identifier</param>
        protected async UniTask SendAsync(IMessage message, string destinationId = DefaultIdentifiers.DESTINATION_ID)
        {
            await Client.SendAsync(Logger, Namespace, message, destinationId);
        }

        /// <summary>
        /// Sends a message and waits the result
        /// </summary>
        /// <typeparam name="TResponse">response type</typeparam>
        /// <param name="message">message to send</param>
        /// <param name="destinationId">destination identifier</param>
        /// <returns>the result</returns>
        protected async UniTask<TResponse> SendAsync<TResponse>(IMessageWithId message, string destinationId = DefaultIdentifiers.DESTINATION_ID) where TResponse : IMessageWithId
        {
            return await Client.SendAsync<TResponse>(Logger, Namespace, message, destinationId);
        }

        /// <summary>
        /// Called when a message for this channel is received
        /// </summary>
        /// <param name="message">message to process</param>
        public virtual UniTask OnMessageReceivedAsync(IMessage message)
        {
            return UniTask.CompletedTask;
        }
    }
}
