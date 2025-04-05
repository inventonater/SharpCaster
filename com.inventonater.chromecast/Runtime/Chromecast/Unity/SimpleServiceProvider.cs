using System;
using System.Collections.Generic;
using System.Linq;

namespace Inventonater.Chromecast.Unity
{
    /// <summary>
    /// A simple service provider implementation for Unity
    /// </summary>
    public class SimpleServiceProvider
    {
        private readonly Dictionary<Type, List<object>> _services = new Dictionary<Type, List<object>>();
        
        /// <summary>
        /// Registers a service with the provider
        /// </summary>
        /// <typeparam name="T">The service type</typeparam>
        /// <param name="implementation">The service implementation</param>
        public void RegisterService<T>(object implementation)
        {
            var type = typeof(T);
            
            if (!_services.TryGetValue(type, out var implementations))
            {
                implementations = new List<object>();
                _services[type] = implementations;
            }
            
            implementations.Add(implementation);
        }
        
        /// <summary>
        /// Gets a service of the specified type
        /// </summary>
        /// <typeparam name="T">The service type</typeparam>
        /// <returns>The service implementation</returns>
        public T GetService<T>() where T : class
        {
            var type = typeof(T);
            
            if (_services.TryGetValue(type, out var implementations) && implementations.Count > 0)
            {
                return implementations[0] as T;
            }
            
            return null;
        }
        
        /// <summary>
        /// Gets all services of the specified type
        /// </summary>
        /// <typeparam name="T">The service type</typeparam>
        /// <returns>The service implementations</returns>
        public IEnumerable<T> GetServices<T>() where T : class
        {
            var type = typeof(T);
            
            if (_services.TryGetValue(type, out var implementations))
            {
                return implementations.Cast<T>();
            }
            
            return Enumerable.Empty<T>();
        }
    }
}
