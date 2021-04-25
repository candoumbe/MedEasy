using Humanizer;

using Identity.API.Features.v1.Accounts;
using Identity.DataStores;
using Identity.DTO;
using Identity.DTO.v1;

using MedEasy.Abstractions.ValueConverters;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NodaTime;
using NodaTime.Serialization.SystemTextJson;

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

using static Newtonsoft.Json.JsonConvert;


namespace Identity.API.Fixtures.v1
{
    public class IdentityApiFixture : IntegrationFixture<Program>
    {
        private readonly static JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

        protected override void ConfigureClient(HttpClient client)
        {
            client.BaseAddress = new Uri("http://localhost");
            client.Timeout = 1.Minutes();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            static DbContextOptionsBuilder<IdentityContext> BuildDbContextOptions(IServiceProvider serviceProvider)
            {
                IHostEnvironment hostingEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();
                DbContextOptionsBuilder<IdentityContext> builder = new();
                builder.UseSqlite("Datasource=:memory:",
                                      options => options.UseNodaTime()
                                                        .MigrationsAssembly(typeof(IdentityContext).Assembly.FullName))
                       .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
                       .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>()
                       .ConfigureWarnings(options =>
                       {
                           options.Default(WarningBehavior.Log);
                       });
                return builder;
            }

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

                services.AddSingleton<IUnitOfWorkFactory, EFUnitOfWorkFactory<IdentityContext>>(serviceProvider =>
                {
                    DbContextOptionsBuilder<IdentityContext> builder = BuildDbContextOptions(serviceProvider);
                    IClock clock = serviceProvider.GetRequiredService<IClock>();
                    IHostEnvironment hostingEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();
                    return new EFUnitOfWorkFactory<IdentityContext>(builder.Options,
                                                                    options =>
                                                                    {
                                                                        IdentityContext context = new IdentityContext(options, clock);
                                                                        context.Database.EnsureCreated();
                                                                        return context;
                                                                    });
                });

            });
        }

        /// <summary>
        /// Register a new account and log with it.
        /// <para>
        /// </para>
        /// </summary>
        /// <param name="newAccount">Account to register</param>
        /// <returns><see cref="BearerTokenInfo"/> elemane which contains bearer token for the newly registered account</returns>
        public async ValueTask<BearerTokenInfo> RegisterAndConnect(NewAccountInfo newAccount)
        {
            // Create account
            using HttpClient client = CreateClient();
            string uri = $"/v1/{AccountsController.EndpointName}";

            using HttpResponseMessage response = await client.PostAsJsonAsync(uri, newAccount, JsonSerializerOptions)
                                                             .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            // Get Token
            return await Connect(new LoginInfo { Username = newAccount.Username, Password = newAccount.Password })
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Connects a previously registered accouunt
        /// </summary>
        /// <param name="loginInfo"></param>
        /// <returns><see cref="BearerTokenInfo"/> elemane which contains bearer token for the newly registered account</returns>
        public async Task<BearerTokenInfo> Connect(LoginInfo loginInfo)
        {
            using HttpClient client = CreateClient();
            const string uri = "/v1/auth/token";

            using HttpResponseMessage response = await client.PostAsJsonAsync(uri, loginInfo, JsonSerializerOptions)
                                                             .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string tokenJson = await response.Content.ReadAsStringAsync()
                                             .ConfigureAwait(false);

            return DeserializeObject<BearerTokenInfo>(tokenJson);
        }
    }
}
