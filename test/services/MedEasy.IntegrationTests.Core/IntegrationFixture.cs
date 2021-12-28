namespace MedEasy.IntegrationTests.Core
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Xunit;

    public class IntegrationFixture<TEntryPoint> : WebApplicationFactory<TEntryPoint>, IAsyncLifetime
        where TEntryPoint : class
    {
        /// <summary>
        /// Name of the scheme used to fake a successfull authentication.
        /// </summary>
        public const string Scheme = "FakeAuthentication";
        private IHost _host;

        ///<inheritdoc/>
        protected override IHostBuilder CreateHostBuilder()
            => base.CreateHostBuilder()
                       .UseEnvironment("IntegrationTest");

        ///<inheritdoc/>
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("IntegrationTest")
                    .UseTestServer()
                    .ConfigureTestServices(services =>
                    {
                        services.AddControllers(opts =>
                        {
                            AuthorizeFilter[] authorizeFilters = opts.Filters.OfType<AuthorizeFilter>()
                                                                            .ToArray();

                            foreach (IFilterMetadata item in authorizeFilters)
                            {
                                opts.Filters.Remove(item);
                            }
                        });

                        //services.Remove<IAuthenticationService>();
                        //services.Remove<IAuthenticationHandlerProvider>();

                        //services.AddTransient((_) => new DummyClaimsProvider(Scheme, Enumerable.Empty<Claim>()))
                        //        .AddAuthorization(opts =>
                        //        {
                        //            if (opts.GetPolicy("Test") is null)
                        //            {
                        //                opts.AddPolicy("Test", new AuthorizationPolicyBuilder(Scheme).RequireAuthenticatedUser()
                        //                                                                             .Build());
                        //            }
                        //        })
                        //        .AddAuthentication(Scheme)
                        //        .AddScheme<AuthenticationSchemeOptions, DummyAuthenticationHandler>(Scheme, opts => { });
                    })
                    .UseUrls("http://*:0", "https://*:0");
        }

        ///<inheritdoc/>
        public async virtual Task InitializeAsync()
        {
            _host = CreateHostBuilder().Build();

            await _host.InitAsync().ConfigureAwait(false);
            await _host.StartAsync().ConfigureAwait(false);
        }

        ///<inheritdoc/>
        public override async ValueTask DisposeAsync() => await DisposeAndStopAsync().ConfigureAwait(false);

        private async Task DisposeAndStopAsync()
        {
            if (_host is not null)
            {
                IServiceScope scope = _host.Services.CreateScope();
                IEnumerable<DbContext> dbContexts = scope.ServiceProvider.GetServices<DbContext>();

                foreach (DbContext dbContext in dbContexts)
                {
                    dbContext.Database?.EnsureDeleted();
                }

                await _host.StopAsync().ConfigureAwait(false);
                _host.Dispose();
            }
            _host = null;
#pragma warning disable CA1816 // Les méthodes Dispose doivent appeler SuppressFinalize
            GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Les méthodes Dispose doivent appeler SuppressFinalize
        }

        ///<inheritdoc/>
       async Task IAsyncLifetime.DisposeAsync() => await DisposeAndStopAsync().ConfigureAwait(false);
    }
}