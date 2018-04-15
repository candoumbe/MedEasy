using Consul;
using FakeItEasy;
using MedEasy.Core.Infrastructure;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit.Abstractions;
using System.Linq;
using static Moq.MockBehavior;
using Xunit;
using Xunit.Categories;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.Threading;
using Microsoft.AspNetCore.Http.Features;

namespace MedEasy.Core.UnitTests.Infrastructure
{
    [UnitTest]
    public class ConsulHostedServiceTests : IDisposable
    {
        private readonly ITestOutputHelper _outputHelper;
        private IServer _server;
        private IConsulClient _consulClient;
        private IOptions<ConsulConfig> _consulOptions;
        private ILogger<ConsulHostedService> _logger;
        private ConsulHostedService _sut;

        public ConsulHostedServiceTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _server = A.Fake<IServer>(options => options.Strict());
            _consulClient = A.Fake<IConsulClient>();
            _consulOptions = A.Fake<IOptions<ConsulConfig>>(options => options.Strict());
            _logger = A.Fake<ILogger<ConsulHostedService>>();

            _sut = new ConsulHostedService(_server, _consulClient, _consulOptions, logger: _logger);
        }


        public void Dispose()
        {
            _server = null;
            _consulClient = null;
            _consulOptions = null;
            _logger = null;
            _sut = null;
        }



        public static IEnumerable<object[]> CtorThrowsArgumentNullExceptionCases
        {
            get
            {
                IServer[] servers = { null, A.Fake<IServer>() };
                IConsulClient[] consulClients = { null, A.Fake<IConsulClient>() };
                IOptions<ConsulConfig>[] consulConfigs = { null, A.Fake<IOptions<ConsulConfig>>() };
                ILogger<ConsulHostedService>[] loggers = { null, A.Fake<ILogger<ConsulHostedService>>() };

                return servers.CrossJoin(consulClients, (server, consulClient) => new { server, consulClient })
                    .CrossJoin(consulConfigs, (tuple, consulConfig) => new { tuple.server, tuple.consulClient, consulConfig })
                    .CrossJoin(loggers, (tuple, logger) => new { tuple.server, tuple.consulClient, tuple.consulConfig, logger })
                    .Where(tuple => tuple.server == null || tuple.consulClient == null || tuple.consulConfig == null || tuple.logger == null)
                    .Select(tuple => new object[]{ tuple.server, tuple.consulClient, tuple.consulConfig, tuple.logger });


            }
        }

        [Theory]
        [MemberData(nameof(CtorThrowsArgumentNullExceptionCases))]
        public void GivenNullParameter_Ctor_ThrowsArgumentNullException(IServer server, IConsulClient consulClient, IOptions<ConsulConfig> consulOptions, ILogger<ConsulHostedService> logger)
        {
            // Act
            Action action = () => new ConsulHostedService(server, consulClient, consulOptions, logger);


            // Assert
            action.ShouldThrow<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task StartAsync()
        {

            // Arrange
            string serverAddress = "http://127.0.0.1";

            IFeatureCollection featureCollection = A.Fake<IFeatureCollection>(options => options.Strict());
            A.CallTo(() => _server.Features)
                .Returns(featureCollection);

            IServerAddressesFeature serverAddressesFeature = A.Fake<IServerAddressesFeature>();
            A.CallTo(() => featureCollection.Get<IServerAddressesFeature>())
                .Returns(serverAddressesFeature);

            A.CallTo(() => serverAddressesFeature.Addresses)
                .Returns(new[] { serverAddress });

            ConsulConfig consulConfig = new ConsulConfig
            {
                Address = "http://localhost",
                ServiceName = "MyUsefullService",
                ServiceID = $"MyUsefullService-{Guid.NewGuid()}",
                Tags = new []{ "api", "service" },
                Check = new ConsultCheckConfig
                {
                    HealthEndpoint = "/health/status",
                    Timeout = 30,
                    Interval = 10
                }
            };

            A.CallTo(() => _consulOptions.Value).Returns(consulConfig);

           

            // Act
            await _sut.StartAsync(default)
                .ConfigureAwait(false);


            // Assert
            A.CallTo(_logger)
                .MustHaveHappenedOnceExactly();


            A.CallTo(() => _consulOptions.Value)
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _server.Features).MustHaveHappenedOnceExactly();

            A.CallTo(() => _consulClient.Agent.ServiceRegister(A<AgentServiceRegistration>.That.Matches(registration =>
                registration.Address == _server.Features.Get<IServerAddressesFeature>().Addresses.First()
                && registration.Name == _consulOptions.Value.ServiceName),

                A<CancellationToken>._))
                    .MustHaveHappenedOnceExactly();
            A.CallTo(() => _consulClient.Agent.ServiceDeregister(A<string>._, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();

        }

        [Fact]
        public async Task StopAsync()
        {
            // Arrange
            string serverAddress = "http://127.0.0.1";

            IFeatureCollection featureCollection = A.Fake<IFeatureCollection>(options => options.Strict());
            A.CallTo(() => _server.Features)
                .Returns(featureCollection);

            IServerAddressesFeature serverAddressesFeature = A.Fake<IServerAddressesFeature>();
            A.CallTo(() => featureCollection.Get<IServerAddressesFeature>())
                .Returns(serverAddressesFeature);

            A.CallTo(() => serverAddressesFeature.Addresses)
                .Returns(new[] { serverAddress });

            ConsulConfig consulConfig = new ConsulConfig
            {
                Address = "http://localhost",
                ServiceName = "MyUsefullService",
                ServiceID = $"MyUsefullService-{Guid.NewGuid()}",
                Tags = new[] { "api", "service" }
            };

            A.CallTo(() => _consulOptions.Value).Returns(consulConfig);

            await _sut.StartAsync(default)
                .ConfigureAwait(false);


            // Act
            await _sut.StopAsync(default)
                .ConfigureAwait(false);


            // Assert
            A.CallTo(_logger)
                .MustHaveHappenedTwiceExactly();


            A.CallTo(() => _consulClient.Agent.ServiceDeregister(A<string>.That.IsNotNull(), A<CancellationToken>._))
                .MustHaveHappenedTwiceExactly();

        }
    }
}
