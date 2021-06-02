
using System.Collections.Generic;

using Yarp.ReverseProxy.Abstractions;
using Yarp.ReverseProxy.Service;

namespace MedEasy.ReverseProxy
{
    /// <summary>
    /// <see cref="IPro"/>
    /// </summary>
    public class TyeConfigurationProvider : IProxyConfigProvider
    {
        private volatile TyeConfig _config;

        public TyeConfigurationProvider(IReadOnlyList<ProxyRoute> routes, IReadOnlyList<Cluster> clusters) => _config = new TyeConfig(routes, clusters);

        /// <summary>
        /// Implementation of the IProxyConfigProvider.GetConfig method to supply the current snapshot of configuration
        /// </summary>
        /// <returns>An immutable snapshot of the current configuration state</returns>
        public IProxyConfig GetConfig() => _config;

        /// <summary>
        /// Swaps the config state with a new snapshot of the configuration, then signals the change
        /// </summary>
        public void Update(IReadOnlyList<ProxyRoute> routes, IReadOnlyList<Cluster> clusters)
        {
            var oldConfig = _config;
            _config = new TyeConfig(routes, clusters);
            oldConfig.SignalChange();
        }

    }
}
