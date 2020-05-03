using Consul;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace MedEasy.Core.Infrastructure
{
    /// <summary>
    /// <see cref="IHostedService"/> implemenation for automatic registration on a Consul host (<see cref="https://www.consul.io/"/> for more details). 
    /// </summary>
    public class ConsulHostedService : IHostedService
    {
        private readonly IServer _server;
        private readonly IConsulClient _consulClient;
        private readonly IOptions<ConsulConfig> _consulOptions;
        private readonly ILogger<ConsulHostedService> _logger;
        private CancellationTokenSource _cts;
        private string _registrationID;

        /// <summary>
        /// Builds a new <see cref="ConsulHostedService"/> instance
        /// </summary>
        /// <param name="server">Server that host the service</param>
        /// <param name="consulClient"></param>
        /// <param name="consulOptions">Options used when registering the service</param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException">if any parameter is <c>null</c>.</exception>
        public ConsulHostedService(IServer server, IConsulClient consulClient, IOptions<ConsulConfig> consulOptions, ILogger<ConsulHostedService> logger)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
            _consulClient = consulClient ?? throw new ArgumentNullException(nameof(consulClient));
            _consulOptions = consulOptions ?? throw new ArgumentNullException(nameof(consulOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Create a linked token so we can trigger cancellation outside of this token's cancellation
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            IServerAddressesFeature addresses = _server.Features.Get<IServerAddressesFeature>();
            string address = addresses.Addresses.First();

            Uri uri = new Uri(address);
            ConsulConfig consulConfig = _consulOptions.Value;
            _registrationID = $"{consulConfig.ServiceID}-{uri.Port}";

            AgentServiceRegistration registration = new AgentServiceRegistration()
            {
                ID = _registrationID,
                Name = consulConfig.ServiceName,
                Address = $"{uri.Scheme}://{uri.Host}",
                Port = uri.Port,
                Tags = consulConfig.Tags
            };

            if (consulConfig.Check != null)
            {
                registration.Check = new AgentServiceCheck()
                {
                    HTTP = $"{uri.Scheme}://{uri.Host}:{uri.Port}{consulConfig.Check.HealthEndpoint}",
                    Timeout = TimeSpan.FromSeconds(consulConfig.Check.Timeout),
                    Interval = TimeSpan.FromSeconds(consulConfig.Check.Interval)                    
                };
            }

            _logger.LogInformation("Registering in Consul");
            // Deregister any previously registerd instance
            await _consulClient.Agent.ServiceDeregister(registration.ID, _cts.Token)
                .ConfigureAwait(false);
            await _consulClient.Agent.ServiceRegister(registration, _cts.Token)
                .ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
            _logger.LogInformation("Deregistering from Consul");
            try
            {
                await _consulClient.Agent.ServiceDeregister(_registrationID, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Deregisteration failed");
            }
        }
    }
}
