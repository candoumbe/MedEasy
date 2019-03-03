using Identity.API.Features.Accounts;
using Identity.DataStores.SqlServer;
using Identity.DTO;
using MedEasy.IntegrationTests.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using static Newtonsoft.Json.JsonConvert;

namespace Identity.API.Fixtures
{
    public class IdentityApiFixture : IntegrationFixture<Startup>
    {
        protected override void ConfigureClient(HttpClient client)
        {
            client.BaseAddress = new Uri("http://localhost/");
            client.Timeout = TimeSpan.FromMinutes(2);
        }

        /// <summary>
        /// Register a new account
        /// </summary>
        /// <param name="newAccount">Account to register</param>
        /// <returns><see cref="BearerTokenInfo"/> elemane which contains bearer token for the newly registered account</returns>
        public async ValueTask<BearerTokenInfo> Register(NewAccountInfo newAccount)
        {
            // Create account
            using (HttpClient client = CreateClient())
            {
                HttpResponseMessage response = await client.PostAsJsonAsync($"/identity/{AccountsController.EndpointName}", newAccount)
                    .ConfigureAwait(false);

                string responseContent = await response.Content.ReadAsStringAsync()
                    .ConfigureAwait(false);
                Debug.WriteLine($"Response : {responseContent}");

                response.EnsureSuccessStatusCode();

                // Get Token
                return await Connect(new LoginInfo { Username = newAccount.Username, Password = newAccount.Password })
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Connects a previously registered accouunt
        /// </summary>
        /// <param name="loginInfo"></param>
        /// <returns><see cref="BearerTokenInfo"/> elemane which contains bearer token for the newly registered account</returns>
        public async Task<BearerTokenInfo> Connect(LoginInfo loginInfo)
        {
            using (HttpClient client = CreateClient())
            {
                HttpResponseMessage response = await client.PostAsJsonAsync($"/auth/token", loginInfo)
                    .ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                string tokenJson = await response.Content.ReadAsStringAsync()
                    .ConfigureAwait(false);

                return DeserializeObject<BearerTokenInfo>(tokenJson);
            }
        }
    }
}
