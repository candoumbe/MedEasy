using Identity.API.Features.Accounts;
using Identity.DataStores.SqlServer;
using Identity.DTO;
using Identity.DTO.v1;
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
        private string _version;

        public IdentityApiFixture() => _version = "1";

        protected override void ConfigureClient(HttpClient client)
        {
            client.BaseAddress = new Uri("http://localhost");
            client.Timeout = TimeSpan.FromMinutes(2);
        }

        public void UseVersion(string version = "1") => _version = version;

        /// <summary>
        /// Register a new account
        /// </summary>
        /// <param name="newAccount">Account to register</param>
        /// <returns><see cref="BearerTokenInfo"/> elemane which contains bearer token for the newly registered account</returns>
        public async ValueTask<BearerTokenInfo> Register(NewAccountInfo newAccount, string version = null)
        {
            // Create account
            using (HttpClient client = CreateClient())
            {
                HttpResponseMessage response = await client.PostAsJsonAsync($"/v{version ?? _version}/{AccountsController.EndpointName}", newAccount)
                    .ConfigureAwait(false);

                string responseContent = await response.Content.ReadAsStringAsync()
                    .ConfigureAwait(false);
                Debug.WriteLine($"Response : {responseContent}");

                response.EnsureSuccessStatusCode();
                response.Dispose();

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
        public async Task<BearerTokenInfo> Connect(LoginInfo loginInfo, string version = null)
        {
            using (HttpClient client = CreateClient())
            {
                HttpResponseMessage response = await client.PostAsJsonAsync($"/auth/v{version ?? _version}/token", loginInfo)
                    .ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                string tokenJson = await response.Content.ReadAsStringAsync()
                    .ConfigureAwait(false);

                return DeserializeObject<BearerTokenInfo>(tokenJson);
            }
        }
    }
}
