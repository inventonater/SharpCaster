using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Inventonater.Chromecast.Interfaces;
using System;

namespace Inventonater.Chromecast.Channels
{
    /// <summary>
    /// Base implementation for channels that maintain a status
    /// </summary>
    /// <typeparam name="TStatus">The status type</typeparam>
    public abstract class StatusChannel<TStatus> : ChromecastChannel, IStatusChannel<TStatus> where TStatus : class
    {
        private TStatus _status;
        
        /// <summary>
        /// Event raised when the status changes
        /// </summary>
        public event EventHandler<TStatus> StatusChanged;
        
        /// <summary>
        /// Gets or sets the status
        /// </summary>
        public TStatus Status
        {
            get => _status;
            set
            {
                var oldStatus = _status;
                _status = value;
                
                if (oldStatus != value)
                {
                    OnStatusChanged(value);
                }
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the StatusChannel class
        /// </summary>
        /// <param name="ns">The namespace</param>
        /// <param name="logger">The logger</param>
        /// <param name="useBaseNamespace">Whether to use the base namespace</param>
        protected StatusChannel(string ns, ILogger logger, bool useBaseNamespace = true) 
            : base(ns, logger, useBaseNamespace)
        {
        }
        
        /// <summary>
        /// Called when the status changes
        /// </summary>
        /// <param name="status">The new status</param>
        protected virtual void OnStatusChanged(TStatus status)
        {
            // Use Unity's main thread to invoke the event
            UniTask.RunOnUiThread(() => 
            {
                StatusChanged?.Invoke(this, status);
            }).Forget();
        }
    }
}
