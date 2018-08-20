using FluentAssertions;
using FluentAssertions.Extensions;
using Identity.DataStores.SqlServer;
using Identity.DTO;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Microsoft.AspNetCore.Http.HttpMethods;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Newtonsoft.Json.JsonConvert;
using static System.Text.Encoding;

namespace Identity.API.IntegrationTests.Features.Auth
{
    [IntegrationTest]
    [Feature("Authentication")]
    public class TokenControllerTests : IDisposable, IClassFixture<ServicesTestFixture<Startup>>, IClassFixture<SqliteDatabaseFixture>
    {
        private ITestOutputHelper _outputHelper;
        private TestServer _server;
        private readonly string _endpointUrl = "/identity";

        public TokenControllerTests(ITestOutputHelper outputHelper, ServicesTestFixture<Startup> identityFixture, SqliteDatabaseFixture sqliteDatabaseFixture)
        {
            _outputHelper = outputHelper;
            identityFixture.Initialize(
                relativeTargetProjectParentDir: Path.Combine("..", "..", "..", "..", "src", "services", "Identity"),
                environmentName: "IntegrationTest",
                applicationName: typeof(Startup).Assembly.GetName().Name);

            _server = identityFixture.Server;
        }

        public void Dispose()
        {
            _outputHelper = null;
            _server = null;
        }

        [Fact]
        public async Task GivenExpiredAccessToken_Calling_Api_Returns_Unauthorized()
        {
            // Arrange
            const string password = "thecapedcrusader";
            NewAccountInfo newAccountInfo = new NewAccountInfo
            {
                Name = "Bruce Wayne",
                Username = "thebatman",
                Password = password,
                ConfirmPassword = password,
                Email = "bruce.wayne@gotham.com"
            };

            RequestBuilder request = _server.CreateRequest($"{_endpointUrl}/accounts")
                .And(msg => msg.Content = new StringContent(SerializeObject(newAccountInfo), UTF8, "application/json"));

            await request.PostAsync()
                .ConfigureAwait(false);

            LoginInfo loginInfo = new LoginInfo
            {
                Username = newAccountInfo.Username,
                Password = newAccountInfo.Password
            };

            request = _server.CreateRequest("/auth/token")
                .And(msg => msg.Content = new StringContent(SerializeObject(loginInfo), UTF8, "application/json"));

            HttpResponseMessage response = await request.PostAsync()
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"Status code : {response.StatusCode}");

            response.IsSuccessStatusCode.Should()
                .BeTrue();
            response.StatusCode.Should()
                .Be(Status200OK);

            string json = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"HTTP content : '{json}'");

            BearerTokenInfo tokenInfo = JToken.Parse(json)
                .ToObject<BearerTokenInfo>();

            SecurityToken accessToken = new JwtSecurityToken(tokenInfo.AccessToken);
            TimeSpan accessDuration = accessToken.ValidTo - accessToken.ValidFrom;
            _outputHelper.WriteLine($"Access token valid from <{accessToken.ValidFrom}> to <{accessToken.ValidTo}>");
            _outputHelper.WriteLine($"The access token will expire in {accessDuration.TotalSeconds} seconds");
            _outputHelper.WriteLine($"Waiting for the token to expire");

            SecurityToken refreshToken = new JwtSecurityToken(tokenInfo.RefreshToken);

            // wait for the access token to expire
            Thread.Sleep(accessDuration + 1.Seconds());

            _outputHelper.WriteLine($"[{DateTime.UtcNow}] access token has epired");

            string path = $"/identity/accounts/?{new PaginationConfiguration { Page = 1, PageSize = 10 }.ToQueryString()}";
            _outputHelper.WriteLine($"Test URL : <{path}>");
            request = _server.CreateRequest(path)
                .AddHeader("Authorization", $"Bearer {tokenInfo.AccessToken}");

            // Act
            response = await request.SendAsync(Head)
                .ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should()
                .BeFalse("The access token has expired");
            response.StatusCode.Should()
                .Be(Status401Unauthorized, "The token has expired");
        }

        [Fact]
        public async Task GivenUserExists_Token_ReturnsValidToken()
        {
            // Arrange
            const string password = "thecapedcrusader";
            NewAccountInfo newAccountInfo = new NewAccountInfo
            {
                Name = "Bruce Wayne",
                Username = "thebatman",
                Password = password,
                ConfirmPassword = password,
                Email = "bruce.wayne@gotham.com"
            };

            RequestBuilder request = _server.CreateRequest($"{_endpointUrl}/accounts")
                .And(msg => msg.Content = new StringContent(SerializeObject(newAccountInfo), UTF8, "application/json"));

            await request.PostAsync()
                .ConfigureAwait(false);

            LoginInfo loginInfo = new LoginInfo
            {
                Username = newAccountInfo.Username,
                Password = newAccountInfo.Password
            };

            request = _server.CreateRequest("/auth/token")
                .And(msg => msg.Content = new StringContent(SerializeObject(loginInfo), UTF8, "application/json"));

            // Act
            HttpResponseMessage response = await request.PostAsync()
                .ConfigureAwait(false);

            // Assert
            _outputHelper.WriteLine($"Status code : {response.StatusCode}");

            response.IsSuccessStatusCode.Should()
                .BeTrue();
            response.StatusCode.Should()
                .Be(Status200OK);

            string json = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"HTTP content : '{json}'");

            BearerTokenInfo tokenInfo = JToken.Parse(json)
                .ToObject<BearerTokenInfo>();
            SecurityToken accessToken = new JwtSecurityToken(tokenInfo.AccessToken);
            SecurityToken refreshToken = new JwtSecurityToken(tokenInfo.RefreshToken);
            refreshToken.ValidFrom.Should()
                .Be(accessToken.ValidFrom, "access and refresh tokens be valid since the same point in time");
            refreshToken.ValidTo.Should()
                .BeAfter(accessToken.ValidTo, "refresh token should expire AFTER access token");
        }

        [Fact]
        public async Task GivenValidAccessToken_Calling_Invalidate_Make_Token_Invalid()
        {
            // Arrange
            const string password = "thecapedcrusader";
            NewAccountInfo newAccountInfo = new NewAccountInfo
            {
                Name = "Bruce Wayne",
                Username = "thebatman",
                Password = password,
                ConfirmPassword = password,
                Email = "bruce.wayne@gotham.com"
            };

            RequestBuilder request = _server.CreateRequest($"{_endpointUrl}/accounts")
                .And(msg => msg.Content = new StringContent(SerializeObject(newAccountInfo), UTF8, "application/json"));

            await request.PostAsync()
                .ConfigureAwait(false);

            LoginInfo loginInfo = new LoginInfo
            {
                Username = newAccountInfo.Username,
                Password = newAccountInfo.Password
            };

            request = _server.CreateRequest("/auth/token")
                .And(msg => msg.Content = new StringContent(SerializeObject(loginInfo), UTF8, "application/json"));

            HttpResponseMessage response = await request.PostAsync()
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"Status code : {response.StatusCode}");

            response.IsSuccessStatusCode.Should()
                .BeTrue();
            response.StatusCode.Should()
                .Be(Status200OK);

            string json = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"HTTP content : '{json}'");

            BearerTokenInfo tokenInfo = JToken.Parse(json)
                .ToObject<BearerTokenInfo>();

            _outputHelper.WriteLine($"access token has epired");

            request = _server.CreateRequest($"/identity/accounts/{newAccountInfo.Username}")
                .AddHeader("Authorization", "");
            // Act
            response = await request.SendAsync(Patch)
                .ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should()
                .BeFalse("The access token has expired");
            response.StatusCode.Should()
                .Be(Status401Unauthorized, "The token has expired");
        }


    }
}
