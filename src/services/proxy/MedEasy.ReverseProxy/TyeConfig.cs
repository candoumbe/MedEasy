using Microsoft.Extensions.Primitives;

using Yarp.ReverseProxy.Configuration;

namespace MedEasy.ReverseProxy
{
    /// <summary>
    /// Describes a tye config
    /// </summary>
    public class TyeConfig : IProxyConfig
    {
        private readonly CancellationTokenSource _cts = new ();

        /// <summary>
        /// Builds a new <see cref="TyeConfig"/> instance with the specified <paramref name="routes"/> and <paramref name="clusters"/>.
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="clusters"></param>
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
