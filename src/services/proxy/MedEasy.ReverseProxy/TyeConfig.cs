using Microsoft.Extensions.Primitives;

using System.Collections.Generic;
using System.Threading;

using Yarp.ReverseProxy.Abstractions;
using Yarp.ReverseProxy.Service;

namespace MedEasy.ReverseProxy
{
    public class TyeConfig : IProxyConfig
    {
        private readonly CancellationTokenSource _cts = new ();

        public TyeConfig(IReadOnlyList<ProxyRoute> routes, IReadOnlyList<Cluster> clusters)
        {
            Routes = routes;
            Clusters = clusters;
            ChangeToken = new CancellationChangeToken(_cts.Token);
        }

        ///<inheritdoc/>
        public IReadOnlyList<ProxyRoute> Routes { get; }

        ///<inheritdoc/>
        public IReadOnlyList<Cluster> Clusters { get; }

        ///<inheritdoc/>
        public IChangeToken ChangeToken { get; }

        internal void SignalChange() => _cts.Cancel();
    }
}
