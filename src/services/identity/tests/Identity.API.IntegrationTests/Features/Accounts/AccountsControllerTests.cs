namespace Identity.API.IntegrationTests.Features.Accounts
{
    using FluentAssertions;
    using Identity.DTO;
    using MedEasy.RestObjects;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Schema;
    using Newtonsoft.Json.Schema.Generation;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;
    using static System.Net.Http.HttpMethod;
    using static Microsoft.AspNetCore.Http.StatusCodes;
    using System.Net.Http.Headers;
    using Bogus;
    using Identity.API.Features.v1.Accounts;
    using Identity.API.Fixtures.v1;
    using System.Text.Json;
    using NodaTime.Serialization.SystemTextJson;
    using NodaTime;
    using System.Net.Http.Json;
    using Identity.Ids;
    using System.Collections.Generic;
    using System.Linq;
    using System;
    using Xunit.Extensions.AssemblyFixture;

    [IntegrationTest]
    [Feature("Accounts")]
    [Feature("Identity")]
    public class AccountsControllerTests : IAsyncLifetime, IAssemblyFixture<IdentityApiFixture>
    {
        private readonly IdentityApiFixture _identityApiFixture;
        private readonly ITestOutputHelper _outputHelper;
        private readonly Faker _faker;

        private static JsonSerializerOptions JsonSerializerOptions
        {
            get
            {
                JsonSerializerOptions options = new JsonSerializerOptions().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                options.PropertyNameCaseInsensitive = true;
                return options;
            }
        }

        public AccountsControllerTests(ITestOutputHelper outputHelper, IdentityApiFixture identityFixture)
        {
            _faker = new();
            _outputHelper = outputHelper;
            _identityApiFixture = identityFixture;
        }


        ///<inheritdoc/>
        public async Task InitializeAsync() => await _identityApiFixture.LogIn().ConfigureAwait(false);

        ///<inheritdoc/>
        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task GivenNoToken_GetAll_Returns_Unauthorized()
        {
            // Arrange
            HttpRequestMessage headRequest = new(Head, "/accounts?page=1&pageSize=5");
            using HttpClient client = _identityApiFixture.CreateClient();
            client.DefaultRequestHeaders.Add("api-version", "1");

            // Act
            HttpResponseMessage response = await client.SendAsync(headRequest)
                .ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should()
                .BeFalse("no bearer token was provided");

            response.StatusCode.Should()
                .HaveValue(Status401Unauthorized);
        }

        [Fact]
        public async Task GivenAccount_Post_Creates_Record()
        {
            // Arrange
            Faker faker = new();
            string password = faker.Lorem.Word();
            NewAccountInfo newAccount = new()
            {
                Id = AccountId.New(),
                Name = faker.Person.FullName,
                Username = faker.Person.UserName,
                Password = password,
                ConfirmPassword = password,
                Email = faker.Person.Email
            };

            using HttpClient httpClient = _identityApiFixture.CreateClient();
            httpClient.DefaultRequestHeaders.Add("api-version", "1.0");

            _outputHelper.WriteLine($"Address : {httpClient.BaseAddress}");

            // Act
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync(AccountsController.EndpointName, newAccount, JsonSerializerOptions)
                                                                 .ConfigureAwait(false);

            // Assert
            _outputHelper.WriteLine($"Response : {response}");
            response.IsSuccessStatusCode.Should()
                .BeTrue();
            response.StatusCode.Should()
                .HaveValue(Status201Created);

            string jsonResponse = await response.Content.ReadAsStringAsync()
                                                        .ConfigureAwait(false);

            JToken jsonToken = JToken.Parse(jsonResponse);
            JSchema responseSchema = new JSchemaGenerator().Generate(typeof(Browsable<AccountInfo>));
            jsonToken.IsValid(responseSchema);

            _outputHelper.WriteLine($"Json: {jsonResponse}");

            Browsable<AccountInfo> browsableAccountInfo = JsonSerializer.Deserialize<Browsable<AccountInfo>>(jsonResponse, JsonSerializerOptions);

            IEnumerable<Link> links = browsableAccountInfo.Links;
            links.Should()
                .NotBeEmpty().And
                .NotContainNulls().And
                .NotContain(link => string.IsNullOrWhiteSpace(link.Href), $"{nameof(Link.Href)} must be explicitely specified for each link").And
                .NotContain(link => string.IsNullOrWhiteSpace(link.Relation), $"{nameof(Link.Relation)} must be explicitely specified for each link").And
                .NotContain(link => string.IsNullOrWhiteSpace(link.Method), $"{nameof(Link.Method)} must be explicitely specified for each link").And
                .Contain(link => link.Relation == LinkRelation.Self, "a direct link to the resource must be provided").And
                .HaveCount(1);

            Link linkToSelf = links.Single(link => link.Relation == LinkRelation.Self);
            linkToSelf.Method.Should()
                             .Be("GET");

            AccountInfo accountInfo = browsableAccountInfo.Resource;

            accountInfo.Name.Should()
                            .Be(newAccount.Name);
            accountInfo.Email.Should()
                             .Be(newAccount.Email);
            accountInfo.Id.Should()
                          .NotBe(AccountId.Empty);
        }

        [Fact]
        public async Task Given_valid_token_Get_should_returns_list_of_accounts()
        {
            // Arrange
            using HttpClient client = _identityApiFixture.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _identityApiFixture.Tokens.AccessToken);
            client.DefaultRequestHeaders.Add("api-version", "1.0");

            _outputHelper.WriteLine($"bearer : {_identityApiFixture.Tokens.Jsonify()}");
            // Act
            using HttpResponseMessage response = await client.GetAsync("/accounts?page=1")
                                                             .ConfigureAwait(false);

            // Assert
            _outputHelper.WriteLine($"Response : {response}");
            _outputHelper.WriteLine($"Response's content : {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");

            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().HaveValue(Status200OK);

            string jsonResponse = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            JToken jsonToken = JToken.Parse(jsonResponse);
            JSchema responseSchema = new JSchemaGenerator()
                .Generate(typeof(GenericPagedGetResponse<Browsable<AccountInfo>>));
            jsonToken.IsValid(responseSchema);
        }

        //[Fact]
        //public async Task GivenValidToken_Search_Returns_ListOfResutls()
        //{
        //    // Arrange
        //    NewAccountInfo newAccount = new NewAccountInfo
        //    {
        //        Name = "Cyrille NDOUMBE",
        //        Username = "candoumbe",
        //        Password = "candoumbe",
        //        ConfirmPassword = "candoumbe",
        //        Email = "candoumbe@medeasy.fr"
        //    };

        //    BearerTokenInfo bearerInfo = await _identityApiFixture.Register(newAccount)
        //        .ConfigureAwait(false);
        //    SearchAccountInfo search = new SearchAccountInfo
        //    {
        //        UserName = newAccount.Username,
        //        Page = 1,
        //        PageSize = 10
        //    };
        //    string url = $"{_endpointUrl}/accounts/search/?{search.ToQueryString()}";
        //    _outputHelper.WriteLine($"url : <{url}>");

        //    RequestBuilder requestBuilder = _identityApiFixture.Server
        //        .CreateRequest(url)
        //        .AddHeader("Authorization", $"Bearer {bearerInfo.AccessToken}");
        //    _outputHelper.WriteLine($"Request : {requestBuilder.Stringify()}");

        //    // Act
        //    HttpResponseMessage response = await requestBuilder.SendAsync(requestBuilder.)
        //        .ConfigureAwait(false);

        //    // Assert
        //    _outputHelper.WriteLine($"Response : {response}");
        //    response.IsSuccessStatusCode.Should().BeTrue();
        //    response.StatusCode.Should().Be(Status200OK);

        //    string jsonResponse = await response.Content.ReadAsStringAsync()
        //        .ConfigureAwait(false);

        //    JToken jsonToken = JToken.Parse(jsonResponse);
        //    JSchema responseSchema = new JSchemaGenerator()
        //        .Generate(typeof(GenericPagedGetResponse<BrowsableResource<AccountInfo>>));
        //    jsonToken.IsValid(responseSchema);
        //}
    }
}
