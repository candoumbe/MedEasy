using System.Collections.Generic;
using System.Linq;

namespace MedEasy.ReverseProxy
{
    /// <summary>
    /// Proxy configuration for a specific <see cref="MedEasyApi"/>
    /// </summary>
    public class ProxyConfig
    {
        public MatchConfig Match { get; init; }

        public List<TransformConfig> Transforms {get; init;}

        public ProxyConfig()
        {
            Transforms = new List<TransformConfig> ();
        }
    }
}
