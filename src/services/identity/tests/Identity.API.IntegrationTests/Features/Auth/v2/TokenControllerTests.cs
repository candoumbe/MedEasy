namespace Identity.API.IntegrationTests.Features.Auth.v2
{
    using Bogus;

    using FluentAssertions;
    using FluentAssertions.Extensions;

    using Identity.API.Fixtures.v1;
    using Identity.DTO;
    using Identity.DTO.Auth;
    using Identity.DTO.v2;
    using Identity.Ids;

    using MedEasy.RestObjects;

    using Microsoft.IdentityModel.Tokens;

    using Newtonsoft.Json.Linq;

    using NodaTime;
    using NodaTime.Serialization.SystemTextJson;

    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Net.Mime;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;
    using Xunit.Extensions.AssemblyFixture;

    using static Microsoft.AspNetCore.Http.StatusCodes;
    using static System.Net.Http.HttpMethod;

    [IntegrationTest]
    [Feature("Authentication")]
    public class TokenControllerTests : IAssemblyFixture<IdentityApiFixture>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IdentityApiFixture _identityApiFixture;
        private readonly string _accountsEndpointBaseUrl = "/accounts";
        private static JsonSerializerOptions JsonSerializerOptions
        {
            get
            {
                JsonSerializerOptions options = new JsonSerializerOptions().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                options.PropertyNameCaseInsensitive = true;
                return options;
            }
        }

        public TokenControllerTests(ITestOutputHelper outputHelper, IdentityApiFixture identityFixture)
        {
            _outputHelper = outputHelper;
            _identityApiFixture = identityFixture;
        }

        [Fact]
        public async Task Given_expired_access_token_calling_api_should_return_Unauthorized()
        {
            // Arrange
            const string password = "thecapedcrusader";
            NewAccountInfo newAccountInfo = new()
            {
                Id = AccountId.New(),
                Name = "Bruce Wayne",
                Username = "thebatman",
                Password = password,
                ConfirmPassword = password,
                Email = "bruce.wayne@gotham.com"
            };

            using HttpClient client = _identityApiFixture.CreateClient();
            client.DefaultRequestHeaders.Add("api-version", "2");
            await client.PostAsJsonAsync(_accountsEndpointBaseUrl, newAccountInfo)
                    .ConfigureAwait(false);

            LoginInfo loginInfo = new()
            {
                Username = newAccountInfo.Username,
                Password = newAccountInfo.Password
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("/auth/token", loginInfo)
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"Status code : {response.StatusCode}");

            response.IsSuccessStatusCode.Should()
                .BeTrue();
            response.StatusCode.Should()
                .HaveValue(Status200OK);

            string json = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"HTTP content : '{json}'");

            BearerTokenInfo tokenInfo = JToken.Parse(json)
                .ToObject<BearerTokenInfo>();

            SecurityToken accessToken = new JwtSecurityToken(tokenInfo.AccessToken.Token);
            TimeSpan accessDuration = accessToken.ValidTo - accessToken.ValidFrom;
            _outputHelper.WriteLine($"Access token valid from <{accessToken.ValidFrom}> to <{accessToken.ValidTo}>");
            _outputHelper.WriteLine($"The access token will expire in {accessDuration.TotalSeconds} seconds");
            _outputHelper.WriteLine($"Waiting for the token to expire");

            SecurityToken refreshToken = new JwtSecurityToken(tokenInfo.RefreshToken.Token);

            // wait for the access token to expire
            Thread.Sleep(accessDuration + 1.Seconds());

            _outputHelper.WriteLine($"[{DateTime.UtcNow}] access token has expired");

            string path = $"{_accountsEndpointBaseUrl}?{new PaginationConfiguration { Page = 1, PageSize = 10 }.ToQueryString()}";
            _outputHelper.WriteLine($"Test URL : <{path}>");

            HttpRequestMessage request = new(Head, path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenInfo.AccessToken.Token);
            // Act
            response = await client.SendAsync(request)
                .ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should()
                .BeFalse("The access token has expired");
            response.StatusCode.Should()
                .HaveValue(Status401Unauthorized, "The token has expired");
        }

        [Fact]
        public async Task GivenUserExists_token_returns_valid_token()
        {
            // Arrange
            const string password = "thecapedcrusader";
            NewAccountInfo newAccountInfo = new()
            {
                Id = AccountId.New(),
                Name = "Bruce Wayne",
                Username = $"thebatman_{Guid.NewGuid()}",
                Password = password,
                ConfirmPassword = password,
                Email = $"bruce.wayne_{Guid.NewGuid()}@gotham.com"
            };

            using HttpClient client = _identityApiFixture.CreateClient();
            client.DefaultRequestHeaders.Add("api-version", "2");
            await client.PostAsJsonAsync(_accountsEndpointBaseUrl, newAccountInfo)
                        .ConfigureAwait(false);

            LoginInfo loginInfo = new()
            {
                Username = newAccountInfo.Username,
                Password = newAccountInfo.Password
            };

            // Act
            HttpResponseMessage response = await client.PostAsJsonAsync("/auth/token", loginInfo)
                .ConfigureAwait(false);

            // Assert
            string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            _outputHelper.WriteLine($"Response content : {responseContent}");

            _outputHelper.WriteLine($"Status code : {response.StatusCode}");

            response.IsSuccessStatusCode.Should()
                .BeTrue();
            ((int)response.StatusCode).Should()
                .Be(Status200OK);

            string json = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"HTTP content : '{json}'");

            BearerTokenInfo tokenInfo = JToken.Parse(json)
                                              .ToObject<BearerTokenInfo>();
            SecurityToken accessToken = new JwtSecurityToken(tokenInfo.AccessToken.Token);
            SecurityToken refreshToken = new JwtSecurityToken(tokenInfo.RefreshToken.Token);
            refreshToken.ValidFrom.Should()
                                  .Be(accessToken.ValidFrom, "access and refresh tokens be valid since the same point in time");
            refreshToken.ValidTo.Should()
                                .BeAfter(accessToken.ValidTo, "refresh token should expire AFTER access token");
        }

        [Fact]
        public async Task Given_valid_access_token_calling_Invalidate_make_token_invalid()
        {
            // Arrange
            const string password = "thecapedcrusader";
            Faker faker = new();
            NewAccountInfo newAccountInfo = new()
            {
                Id = AccountId.New(),
                Name = faker.Person.FullName,
                Username = $"{faker.Person.UserName}_{Guid.NewGuid()}",
                Password = password,
                ConfirmPassword = password,
                Email = faker.Internet.Email(uniqueSuffix: Guid.NewGuid().ToString())
            };

            using HttpClient client = _identityApiFixture.CreateClient();
            client.DefaultRequestHeaders.Add("api-version", "2");
            HttpResponseMessage response = await client.PostAsJsonAsync(_accountsEndpointBaseUrl, newAccountInfo, JsonSerializerOptions)
                                                       .ConfigureAwait(false);

            _outputHelper.WriteLine($"Response content : {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");
            response.EnsureSuccessStatusCode();

            LoginInfo loginInfo = new()
            {
                Username = newAccountInfo.Username,
                Password = newAccountInfo.Password
            };

            response = await client.PostAsJsonAsync("/auth/token", loginInfo)
                                   .ConfigureAwait(false);

            _outputHelper.WriteLine($"Status code : {response.StatusCode}");

            response.IsSuccessStatusCode.Should()
                .BeTrue();
            response.StatusCode.Should()
                .HaveValue(Status200OK);

            string json = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            BearerTokenInfo tokenInfo = JToken.Parse(json)
                .ToObject<BearerTokenInfo>();

            HttpRequestMessage requestInvalidateToken = new(Delete, $"/auth/token/{newAccountInfo.Username}");
            AuthenticationHeaderValue bearerTokenHeader = new("Bearer", tokenInfo.AccessToken.Token);
            requestInvalidateToken.Headers.Authorization = bearerTokenHeader;
            response = await client.SendAsync(requestInvalidateToken)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            _outputHelper.WriteLine("Refresh token was successfully revoked");

            HttpRequestMessage refreshTokenRequest = new(Put, $"/auth/token/{newAccountInfo.Username}");
            refreshTokenRequest.Headers.Authorization = bearerTokenHeader;

            RefreshAccessTokenInfo refreshAccessTokenInfo = new() { AccessToken = tokenInfo.AccessToken.Token, RefreshToken = tokenInfo.RefreshToken.Token };
            refreshTokenRequest.Content = new StringContent(refreshAccessTokenInfo.Jsonify(), Encoding.UTF8, MediaTypeNames.Application.Json);

            // Act
            response = await client.SendAsync(refreshTokenRequest)
                .ConfigureAwait(false);

            // Assert
            string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            _outputHelper.WriteLine($"Response content : {responseContent}");

            response.IsSuccessStatusCode.Should()
                .BeFalse("The refresh token was revoked and can no longer be used to refresh an access token");
            response.StatusCode.Should()
                .HaveValue(Status401Unauthorized, "The token has expired");
        }
    }
}
