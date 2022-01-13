
using System.Collections.Generic;

using Yarp.ReverseProxy.Configuration;

namespace MedEasy.ReverseProxy
{
    /// <summary>
    /// <see cref="IProxyConfigProvider"/> implementation for tye
    /// </summary>
    public class TyeConfigurationProvider : IProxyConfigProvider
    {
        private volatile TyeConfig _config;

        /// <summary>
        /// Builds a new <see cref="TyeConfigurationProvider"/> for tye
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="clusters"></param>
        public TyeConfigurationProvider(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters) => _config = new TyeConfig(routes, clusters);

        /// <summary>
        /// Implementation of the IProxyConfigProvider.GetConfig method to supply the current snapshot of configuration
        /// </summary>
        /// <returns>An immutable snapshot of the current configuration state</returns>
        public IProxyConfig GetConfig() => _config;

        /// <summary>
        /// Swaps the config state with a new snapshot of the configuration, then signals the change
        /// </summary>
        public void Update(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
        {
            var oldConfig = _config;
            _config = new TyeConfig(routes, clusters);
            oldConfig.SignalChange();
        }
    }
}
