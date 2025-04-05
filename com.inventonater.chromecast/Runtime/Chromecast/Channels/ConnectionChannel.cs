using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Inventonater.Chromecast.Interfaces;
using Inventonater.Chromecast.Messages.Connection;

namespace Inventonater.Chromecast.Channels
{
    /// <summary>
    /// Connection channel, responsible for opening connection to Chromecast and receiving Closed message
    /// </summary>
    public class ConnectionChannel : ChromecastChannel, IConnectionChannel
    {
        /// <summary>
        /// Initializes a new instance of ConnectionChannel class
        /// </summary>
        public ConnectionChannel(ILogger<ConnectionChannel> log = null) : base("tp.connection", log)
        {
        }

        /// <summary>
        /// Connects to chromecast
        /// </summary>
        public async UniTask ConnectAsync()
        {
            await SendAsync(new ConnectMessage());
        }

        /// <summary>
        /// Connects to running chromecast application
        /// </summary>
        /// <param name="transportId">The transport ID</param>
        public async UniTask ConnectAsync(string transportId)
        {
            await SendAsync(new ConnectMessage(), transportId);
        }

        /// <summary>
        /// Disconnects from the Chromecast
        /// </summary>
        public async UniTask DisconnectAsync()
        {
            // Optional: implement disconnect logic if needed
            await UniTask.CompletedTask;
        }

        /// <summary>
        /// Called when a message for this channel is received
        /// </summary>
        /// <param name="message">message to process</param>
        public override async UniTask OnMessageReceivedAsync(IMessage message)
        {
            if (message is CloseMessage)
            {
                // Use UniTask.RunOnThreadPool instead of Task.Run to handle threading in Unity
                UniTask.RunOnThreadPool(async () => await Client.DisconnectAsync()).Forget();
            }
            await base.OnMessageReceivedAsync(message);
        }
    }
}
