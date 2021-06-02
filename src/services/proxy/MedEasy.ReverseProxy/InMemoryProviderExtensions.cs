using Microsoft.Extensions.DependencyInjection;

using System.Collections.Generic;
using System.Linq;

using Yarp.ReverseProxy.Abstractions;
using Yarp.ReverseProxy.Service;

namespace MedEasy.ReverseProxy
{
    public static class InMemoryProviderExtensions
    {
        public static IReverseProxyBuilder LoadFromMemory(this IReverseProxyBuilder builder, IEnumerable<ProxyRoute> routes, IEnumerable<Cluster> clusters)
        {
            builder.Services.AddSingleton<IProxyConfigProvider>(new TyeConfigurationProvider(routes.ToList(), clusters.ToList()));
            return builder;
        }
    }
}
