using Microsoft.Extensions.DependencyInjection;

using System.Collections.Generic;
using System.Linq;

using Yarp.ReverseProxy.Configuration;

namespace MedEasy.ReverseProxy
{
    /// <summary>
    /// Extension methods for configuring a <see cref="IReverseProxyBuilder"/> instance.
    /// </summary>
    public static class InMemoryProviderExtensions
    {
        /// <summary>
        /// Configures <see cref="IReverseProxyBuilder"/> to load its configuration from its memory.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="routes"></param>
        /// <param name="clusters"></param>
        /// <returns></returns>
        public static IReverseProxyBuilder LoadFromMemory(this IReverseProxyBuilder builder, IEnumerable<RouteConfig> routes, IEnumerable<ClusterConfig> clusters)
        {
            builder.Services.AddSingleton<IProxyConfigProvider>(new TyeConfigurationProvider(routes.ToList(), clusters.ToList()));
            return builder;
        }
    }
}
