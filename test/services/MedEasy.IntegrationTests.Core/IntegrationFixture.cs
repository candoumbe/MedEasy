namespace MedEasy.IntegrationTests.Core
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using Refit;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    public class IntegrationFixture<TEntryPoint> : WebApplicationFactory<TEntryPoint>, IAsyncDisposable
        where TEntryPoint : class
    {
        /// <summary>
        /// Name of the scheme used to fake a successfull authentication.
        /// </summary>
        public const string Scheme = "FakeAuthentication";

        private IHost _host;

        ///<inheritdoc/>
        protected override void ConfigureWebHost(IWebHostBuilder builder)
            => builder.UseEnvironment("IntegrationTest")
                      .ConfigureAppConfiguration((hostingBuilder, configBuilder) =>
                      {
                          configBuilder
                              .AddJsonFile("appsettings.json", optional: true)
                              .AddJsonFile($"appsettings.{hostingBuilder.HostingEnvironment.EnvironmentName}.json", true, true)
                              .AddEnvironmentVariables();

                      })
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
                      });

        /// <summary>
        /// Initializes a <see cref="HttpClient"/> instance that can be later used to call
        /// endpoints where authorization/authentication is required
        /// </summary>
        /// <param name="claims">Claims for the authenticate user</param>
        /// <remarks>
        /// <para>
        /// This method removes all <see cref="AuthorizeFilter"/>s and replace it with a
        /// <see cref="DummyAuthenticationHandler"/> that succeeds only if <paramref name="claims"/> is not empty and contains at least one 
        /// non null value</item>
        /// </para>
        /// </remarks>
        public HttpClient CreateAuthenticatedHttpClientWithClaims(IEnumerable<Claim> claims)
        {
            return WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("IntegrationTest");
                builder.ConfigureTestServices(services =>
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

                    services.AddTransient((_) => new DummyClaimsProvider(Scheme, claims))
                            .AddAuthorization(opts =>
                            {
                                opts.AddPolicy("Test", new AuthorizationPolicyBuilder(Scheme).RequireAuthenticatedUser()
                                                                                             .Build());
                            })
                            .AddAuthentication(Scheme)
                            .AddScheme<AuthenticationSchemeOptions, DummyAuthenticationHandler>(Scheme, opts => { });

                });

            })
                            .CreateClient();
        }


        public TRefitClient CreateRefitClient<TRefitClient>(HttpClient http, JsonSerializerOptions serializerOptions = null)
        {
            return RestService.For<TRefitClient>(http,
                                                new RefitSettings
                                                {
                                                    CollectionFormat = CollectionFormat.Multi,
                                                    ContentSerializer = new SystemTextJsonContentSerializer(serializerOptions ?? new JsonSerializerOptions
                                                    {
                                                        AllowTrailingCommas = true,
                                                        PropertyNameCaseInsensitive = true,
                                                        Converters = { new JsonStringEnumConverter() }
                                                    })
                                                });
        }

        ///<inheritdoc/>
        public async Task InitializeAsync()
        {
            _host = CreateHostBuilder().Build();

            await _host.InitAsync().ConfigureAwait(false);

            await _host.StartAsync().ConfigureAwait(false);
        }

        ///<inheritdoc/>
        public async override ValueTask DisposeAsync()
        {
            if (_host is not null)
            {
                DbContext dbContext = _host.Services.GetService<DbContext>();
                dbContext?.Database?.EnsureDeleted();
                await _host.StopAsync().ConfigureAwait(false);
                _host.Dispose();
            }
            _host = null;
            GC.SuppressFinalize(this);
        }


    }
}