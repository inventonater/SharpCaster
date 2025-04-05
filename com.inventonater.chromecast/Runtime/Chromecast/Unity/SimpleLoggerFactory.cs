using Microsoft.Extensions.Logging;
using System;
using UnityEngine;

namespace Inventonater.Chromecast.Unity
{
    /// <summary>
    /// A simple logger factory implementation for Unity
    /// </summary>
    public class SimpleLoggerFactory : ILoggerFactory
    {
        /// <summary>
        /// Creates a new logger for the specified category
        /// </summary>
        public ILogger CreateLogger(string categoryName)
        {
            return new SimpleLogger(categoryName);
        }

        /// <summary>
        /// Adds a provider - not implemented in this simple version
        /// </summary>
        public void AddProvider(ILoggerProvider provider)
        {
            // Not implemented for this simple version
        }

        /// <summary>
        /// Disposes the logger factory
        /// </summary>
        public void Dispose()
        {
            // Nothing to dispose
        }
        
        /// <summary>
        /// A simple logger implementation for Unity
        /// </summary>
        private class SimpleLogger : ILogger
        {
            private readonly string _categoryName;
            
            /// <summary>
            /// Initializes a new instance of the <see cref="SimpleLogger"/> class
            /// </summary>
            public SimpleLogger(string categoryName)
            {
                _categoryName = categoryName;
            }
            
            /// <summary>
            /// Begins a logical operation scope
            /// </summary>
            public IDisposable BeginScope<TState>(TState state)
            {
                return DummyDisposable.Instance;
            }
            
            /// <summary>
            /// Checks if the log level is enabled
            /// </summary>
            public bool IsEnabled(LogLevel logLevel)
            {
                return logLevel != LogLevel.None;
            }
            
            /// <summary>
            /// Logs a message
            /// </summary>
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (!IsEnabled(logLevel))
                    return;
                
                string message = formatter(state, exception);
                
                switch (logLevel)
                {
                    case LogLevel.Trace:
                    case LogLevel.Debug:
                    case LogLevel.Information:
                        Debug.Log($"[{_categoryName}] {message}");
                        break;
                    case LogLevel.Warning:
                        Debug.LogWarning($"[{_categoryName}] {message}");
                        break;
                    case LogLevel.Error:
                    case LogLevel.Critical:
                        Debug.LogError($"[{_categoryName}] {message}");
                        break;
                    default:
                        Debug.Log($"[{_categoryName}] {message}");
                        break;
                }
                
                if (exception != null)
                {
                    Debug.LogException(exception);
                }
            }
            
            /// <summary>
            /// A dummy disposable for the scope
            /// </summary>
            private class DummyDisposable : IDisposable
            {
                public static readonly DummyDisposable Instance = new DummyDisposable();
                
                public void Dispose() { }
            }
        }
    }
}
