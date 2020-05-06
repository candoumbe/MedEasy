using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace MedEasy.IntegrationTests.Core
{
    public class IntegrationFixture<TEntryPoint> : WebApplicationFactory<TEntryPoint> where TEntryPoint : class
    {
        /// <summary>
        /// Name of the scheme used to fake a successfull authentication. 
        /// </summary>
        public const string Scheme = "FakeAuthentication";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
            => builder
                      //.UseContentRoot(".")
                      .UseEnvironment("IntegrationTest");


        /// <summary>
        /// Initializes a <see cref="HttpClient"/> instance that can be later used to call endpoints where authorization/authentication is required
        /// 
        /// <para>
        /// This method removes all <see cref="AuthorizeFilter"/>s and replace it with a <see cref="DummyAuthenticationHandler"/> that succeeds only if 
        /// <list type="bullet">
        ///     <item><paramref name="claims"/> is not empty and contains at least one non null value</item>
        ///     <item><paramref name="claims"/> is not empty and contains at least one non null value</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="claims">Claims for the authenticate user</param>
        /// <returns></returns>
        public HttpClient CreateAuthenticatedHttpClientWithClaims(IEnumerable<Claim> claims)
        {
            return WithWebHostBuilder(builder =>
            {

                builder.ConfigureTestServices(services =>
                {
                    services.AddControllers(opts =>
                    {
                        var authorizeFilters = opts.Filters.OfType<AuthorizeFilter>().ToArray();
                        foreach (IFilterMetadata item in authorizeFilters)
                        {
                            opts.Filters.Remove(item);

                        }
                    });
                    //services.AddTransient<IAuthorizationHandler, DummyAuthorizationHandler>();
                    services.AddTransient<DummyClaimsProvider>((_) => new DummyClaimsProvider(Scheme, claims))
                            .AddAuthorization(opts =>
                            {
                                opts.AddPolicy("Test", new AuthorizationPolicyBuilder(Scheme)
                                                    .RequireAuthenticatedUser().Build());
                            })
                            .AddAuthentication(Scheme)
                            .AddScheme<AuthenticationSchemeOptions, DummyAuthenticationHandler>(Scheme, opts => { });
                });
            })
                            .CreateClient();
        }
    }
}