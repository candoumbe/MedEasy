using Identity.API.Features.v1.Accounts;
using Identity.DataStores;
using Identity.DTO;
using Identity.DTO.Auth;
using Identity.DTO.v2;
using Identity.Ids;

using MedEasy.IntegrationTests.Core;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NodaTime;
using NodaTime.Serialization.SystemTextJson;

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.API.Fixtures.v2
{
    public class IdentityApiFixture : IntegrationFixture<Startup>
    {
        public static JsonSerializerOptions SerializerOptions
        {
            get

            {
                JsonSerializerOptions options = new JsonSerializerOptions().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                options.PropertyNameCaseInsensitive = true;
                return options;
            }
        }

        /// <summary>
        /// Gets/sets the email to use to create an account or to log in
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Password to use to create an account or to login
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///  Name of the account
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The current token
        /// </summary>
        public BearerTokenInfo Tokens { get; private set; }



        protected override void ConfigureClient(HttpClient client)
        {
            client.BaseAddress = new Uri("http://local");
            client.Timeout = TimeSpan.FromMinutes(2);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
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

                services.AddTransient<DummyClaimsProvider>((_) => new DummyClaimsProvider(Scheme, Enumerable.Empty<Claim>()))
                        .AddAuthorization(opts =>
                        {
                            opts.AddPolicy("Test", new AuthorizationPolicyBuilder(Scheme).RequireAuthenticatedUser()
                                                                                         .Build());
                        })
                        .AddAuthentication(Scheme)
                        .AddScheme<AuthenticationSchemeOptions, DummyAuthenticationHandler>(Scheme, opts => { });

                
                ServiceProvider sp = services.BuildServiceProvider();

                using IServiceScope scope = sp.CreateScope();
                IServiceProvider scopedServices = scope.ServiceProvider;
                IdentityContext db = scopedServices.GetRequiredService<IdentityContext>();
                
                db.Database.Migrate();
            });
        }

        /// <summary>
        /// Register a new account
        /// </summary>
        /// <param name="newAccount">Account to register</param>
        private async Task Register(CancellationToken ct = default)
        {
            // Create account
            using HttpClient client = CreateClient();
            string uri = $"/v2/{AccountsController.EndpointName}";

            NewAccountInfo newAccount = new ()
            {
                Email = Email,
                Password = Password,
                ConfirmPassword = Password,
                Name = Email,
                Username = Email,
                Id = AccountId.New()
            };

            using HttpResponseMessage response = await client.PostAsJsonAsync(uri, newAccount, SerializerOptions, ct)
                                                             .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            // Get Token
            await LogIn(ct) .ConfigureAwait(false);
        }

        /// <summary>
        /// Connects a previously registered accouunt
        /// </summary>
        public async Task LogIn(CancellationToken ct = default)
        {
            using HttpClient client = CreateClient();
            const string uri = "/v2/auth/token";

            if (Tokens is null || Tokens.AccessToken.Expires < DateTime.UtcNow)
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(uri, new { Username = Email, Password }, SerializerOptions, ct)
                    .ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    Tokens = await response.Content.ReadFromJsonAsync<BearerTokenInfo>(SerializerOptions, ct)
                                                  .ConfigureAwait(false);

                }
                else 
                {
                    await Register(ct).ConfigureAwait(false);
                }

            }
            else
            {
                await RenewToken(Email, new RefreshAccessTokenInfo { AccessToken = Tokens.AccessToken.Token, RefreshToken = Tokens.RefreshToken.Token }, ct)
                    .ConfigureAwait(false);
            }
            
        }


        /// <summary>
        /// Refreshes access token for the specified <paramref name="username"/>.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="refreshTokenInfo"></param>
        /// <returns></returns>
        public async Task RenewToken(string username, RefreshAccessTokenInfo refreshTokenInfo, CancellationToken ct = default)
        {
            using HttpClient client = CreateClient();
            string uri = $"/v2/auth/token/{username}/refresh";

            HttpResponseMessage response = await client.PostAsJsonAsync(uri, refreshTokenInfo, SerializerOptions, ct)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                Tokens = await response.Content.ReadFromJsonAsync<BearerTokenInfo>(SerializerOptions, ct)
                                         .ConfigureAwait(false);
            }
             
        }
    }
}