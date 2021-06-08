
using Yarp.ReverseProxy.Abstractions;

namespace MedEasy.ReverseProxy
{
    /// <summary>
    /// Wrapper for describing a MedEasy REST API
    /// </summary>
    public class MedEasyApi
    {
        public string Name { get; init; }

        public string Id { get; init; }

        public string Binding { get; init; }

        public ProxyConfig Proxy { get; init; }
    }
}
