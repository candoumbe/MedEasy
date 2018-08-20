using Identity.API.Features.Accounts;
using Identity.DTO;
using MedEasy.IntegrationTests.Core;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Newtonsoft.Json.JsonConvert;

namespace Identity.API.Fixtures
{
#pragma warning disable RCS1102 // Make class static.
    public class IdentityApiFixture
#pragma warning restore RCS1102 // Make class static.
    {
        /// <summary>
        /// Register a new account
        /// </summary>
        /// <param name="newAccount"></param>
        /// <returns><see cref="BearerTokenInfo"/> elemane which contains bearer token for the newly registered account</returns>
        public static async ValueTask<BearerTokenInfo> Register(TestServer identityServer , NewAccountInfo newAccount)
        {
            // Create account
            RequestBuilder requestBuilder = identityServer.CreateRequest($"/identity/{AccountsController.EndpointName}")
                .And(message => message.Content = new StringContent(SerializeObject(newAccount), Encoding.UTF8, "application/json"));

            HttpResponseMessage response = await requestBuilder.PostAsync()
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            // Get Token
            return await Connect(identityServer, new LoginInfo { Username = newAccount.Username, Password = newAccount.Password })
                .ConfigureAwait(false);
            
        }

        /// <summary>
        /// Connects a previously registered accouunt
        /// </summary>
        /// <param name="loginInfo"></param>
        /// <returns><see cref="BearerTokenInfo"/> elemane which contains bearer token for the newly registered account</returns>
        public static async  Task<BearerTokenInfo> Connect(TestServer identityServer, LoginInfo loginInfo)
        {
            RequestBuilder requestBuilder = identityServer.CreateRequest("/auth/token")
                .And(message => message.Content = new StringContent(SerializeObject(loginInfo), Encoding.UTF8, "application/json"));
            HttpResponseMessage response = await requestBuilder.PostAsync()
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string tokenJson = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            return DeserializeObject<BearerTokenInfo>(tokenJson);

        }


    }
}
