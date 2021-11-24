using Microsoft.Extensions.Primitives;

using System.Collections.Generic;
using System.Threading;

using Yarp.ReverseProxy.Configuration;

namespace MedEasy.ReverseProxy
{
    public class TyeConfig : IProxyConfig
    {
        private readonly CancellationTokenSource _cts = new ();

        public TyeConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
        {
            Routes = routes;
            Clusters = clusters;
            ChangeToken = new CancellationChangeToken(_cts.Token);
        }

        ///<inheritdoc/>
        public IReadOnlyList<RouteConfig> Routes { get; }

        ///<inheritdoc/>
        public IReadOnlyList<ClusterConfig> Clusters { get; }

        ///<inheritdoc/>
        public IChangeToken ChangeToken { get; }

        internal void SignalChange() => _cts.Cancel();
    }
}
