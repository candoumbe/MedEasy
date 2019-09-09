using Identity.API.Features.v1.Accounts;
using Identity.DTO;
using Identity.DTO.v2;
using MedEasy.IntegrationTests.Core;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using static Newtonsoft.Json.JsonConvert;


namespace Identity.API.Fixtures.v2
{
    public class IdentityApiFixture : IntegrationFixture<Startup>
    {
        protected override void ConfigureClient(HttpClient client)
        {
            client.BaseAddress = new Uri("http://localhost");
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
                string uri = $"/v1/{AccountsController.EndpointName}";

                HttpResponseMessage response = await client.PostAsJsonAsync(uri, newAccount)
                    .ConfigureAwait(false);

                string responseContent = await response.Content.ReadAsStringAsync()
                    .ConfigureAwait(false);

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
        public async Task<BearerTokenInfo> Connect(LoginInfo loginInfo)
        {
            using (HttpClient client = CreateClient())
            {

                const string uri = "/v2/auth/token";

                HttpResponseMessage response = await client.PostAsJsonAsync(uri, loginInfo)
                    .ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                string tokenJson = await response.Content.ReadAsStringAsync()
                    .ConfigureAwait(false);

                return DeserializeObject<BearerTokenInfo>(tokenJson);
            }
        }
    }
}