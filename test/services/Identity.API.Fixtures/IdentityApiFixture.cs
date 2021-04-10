using Identity.API.Features.v1.Accounts;
using Identity.DTO;
using Identity.DTO.v1;

using MedEasy.IntegrationTests.Core;

using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

using static Newtonsoft.Json.JsonConvert;


namespace Identity.API.Fixtures
{
    [Obsolete("Use the versioned equivalent instead")]
    public class IdentityApiFixture : IntegrationFixture<Startup>
    {
        private string _version;

        public IdentityApiFixture() => _version = "1";

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
