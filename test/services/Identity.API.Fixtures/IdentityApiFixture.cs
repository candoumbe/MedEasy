using Identity.API.Features.v1.Accounts;
using Identity.DataStores;
using Identity.DTO;
using Identity.DTO.v1;

using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NodaTime;

using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

using static Newtonsoft.Json.JsonConvert;


namespace Identity.API.Fixtures
{
    [Obsolete("Use the versioned equivalent instead")]
    public class IdentityApiFixture : IntegrationFixture<Program>
    {
        private readonly string _version;

        public IdentityApiFixture() => _version = "1";

        protected override void ConfigureClient(HttpClient client)
        {
            client.BaseAddress = new Uri("http://localhost");
            client.Timeout = TimeSpan.FromMinutes(2);
        }

        
        protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.ConfigureTestServices(services =>
                                                                             {
                                                                                 services.Remove<IUnitOfWorkFactory>();
                                                                                 services.AddSingleton<IUnitOfWorkFactory, EFUnitOfWorkFactory<IdentityContext>>(serviceProvider =>
                                                                                 {
                                                                                     DbContextOptionsBuilder<IdentityContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<IdentityContext>().UseSqlite("Datasource=:memory:",
                                                                                                   options => options.UseNodaTime()
                                                                                                                     .MigrationsAssembly(typeof(IdentityContext).Assembly.FullName));
                                                                                     IClock clock = serviceProvider.GetRequiredService<IClock>();
                                                                                     return new EFUnitOfWorkFactory<IdentityContext>(dbContextOptionsBuilder.Options,
                                                                                                                                     options =>
                                                                                                                                     {
                                                                                                                                         IdentityContext context = new IdentityContext(options, clock);
                                                                                                                                         context.Database.EnsureCreated();
                                                                                                                                         return context;

                                                                                                                                     });
                                                                                 });
                                                                             });

        /// <summary>
        /// Register a new account
        /// </summary>
        /// <param name="newAccount">Account to register</param>
        /// <returns><see cref="BearerTokenInfo"/> elemane which contains bearer token for the newly registered account</returns>
        public async ValueTask<BearerTokenInfo> Register(NewAccountInfo newAccount, string version = null)
        {
            // Create account
            using HttpClient client = CreateClient();
            string uri = $"/v{version ?? _version}/{AccountsController.EndpointName}";

            using HttpResponseMessage response = await client.PostAsync(uri, new StringContent(newAccount.Jsonify(), Encoding.UTF8, MediaTypeNames.Application.Json))
                                                             .ConfigureAwait(false);

            string responseContent = await response.Content.ReadAsStringAsync()
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
        public async Task<BearerTokenInfo> Connect(LoginInfo loginInfo, string version = null)
        {
            using HttpClient client = CreateClient();
            string uri = $"v{version ?? _version}/auth/token";

            HttpResponseMessage response = await client.PostAsync(uri, new StringContent(loginInfo.Jsonify(), Encoding.UTF8, MediaTypeNames.Application.Json))
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string tokenJson = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            return DeserializeObject<BearerTokenInfo>(tokenJson);
        }
    }

}
