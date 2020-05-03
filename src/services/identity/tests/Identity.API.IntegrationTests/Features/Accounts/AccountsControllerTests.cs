using FluentAssertions;
using Identity.DTO;
#if NETCOREAPP2_0
using MedEasy.IntegrationTests.Core; 
#else
#endif
using MedEasy.RestObjects;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static System.Net.Http.HttpMethod;
using static Microsoft.AspNetCore.Http.StatusCodes;
using System.Net.Http.Headers;
using Bogus;
using Identity.DTO.v1;
using Identity.API.Features.v1.Accounts;
using Identity.API.Fixtures.v1;

namespace Identity.API.IntegrationTests.Features.Accounts
{
    [IntegrationTest]
    [Feature("Accounts")]
    [Feature("Identity")]
    public class AccountsControllerTests : IClassFixture<IdentityApiFixture>
    {
        private IdentityApiFixture _identityApiFixture;
        private ITestOutputHelper _outputHelper;
        private const string _endpointUrl = "/v1";

        public AccountsControllerTests(ITestOutputHelper outputHelper, IdentityApiFixture identityFixture)
        {
            _outputHelper = outputHelper;
            _identityApiFixture = identityFixture;
        }

        [Fact]
        public async Task GivenNoToken_GetAll_Returns_Unauthorized()
        {
            // Arrange
            HttpRequestMessage headRequest = new HttpRequestMessage(Head, $"{_endpointUrl}/accounts?page=1&pageSize=5");
            using (HttpClient client = _identityApiFixture.CreateClient())
            {
                // Act
                HttpResponseMessage response = await client.SendAsync(headRequest)
                    .ConfigureAwait(false);

                // Assert
                response.IsSuccessStatusCode.Should()
                    .BeFalse("no bearer token was provided");

                response.StatusCode.Should()
                    .Be(Status401Unauthorized);
            }
        }

        [Fact]
        public async Task GivenAccount_Post_Creates_Record()
        {
            // Arrange
            Faker faker = new Faker();
            string password = faker.Lorem.Word();
            NewAccountInfo newAccount = new NewAccountInfo
            {
                Name = faker.Person.FullName,
                Username = faker.Person.UserName,
                Password = password,
                ConfirmPassword = password,
                Email = faker.Person.Email
            };

            using HttpClient httpClient = _identityApiFixture.CreateClient();
            _outputHelper.WriteLine($"Address : {httpClient.BaseAddress}");

            // Act
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{_endpointUrl}/{AccountsController.EndpointName}", newAccount)
                                                                 .ConfigureAwait(false);

            // Assert
            _outputHelper.WriteLine($"Response : {response}");
            response.IsSuccessStatusCode.Should()
                .BeTrue();
            response.StatusCode.Should()
                .Be(Status201Created);

            string jsonResponse = await response.Content.ReadAsStringAsync()
                                                        .ConfigureAwait(false);

            JToken jsonToken = JToken.Parse(jsonResponse);
            JSchema responseSchema = new JSchemaGenerator().Generate(typeof(Browsable<AccountInfo>));
            jsonToken.IsValid(responseSchema);

            Browsable<AccountInfo> browsableResource = jsonToken.ToObject<Browsable<AccountInfo>>();

            IEnumerable<Link> links = browsableResource.Links;
            links.Should()
                .NotBeEmpty().And
                .NotContainNulls().And
                .NotContain(link => string.IsNullOrWhiteSpace(link.Href), $"{nameof(Link.Href)} must be explicitely specified for each link").And
                .NotContain(link => string.IsNullOrWhiteSpace(link.Relation), $"{nameof(Link.Relation)} must be explicitely specified for each link").And
                .NotContain(link => string.IsNullOrWhiteSpace(link.Method), $"{nameof(Link.Method)} must be explicitely specified for each link").And
                .Contain(link => link.Relation == LinkRelation.Self, $"a direct link to the resource must be provided").And
                .HaveCount(1);

            Link linkToSelf = links.Single(link => link.Relation == LinkRelation.Self);
            linkToSelf.Method.Should()
                             .Be("GET");

            AccountInfo createdAccountInfo = browsableResource.Resource;

            createdAccountInfo.Name.Should()
                                   .Be(newAccount.Name);
            createdAccountInfo.Email.Should()
                .Be(newAccount.Email);

            createdAccountInfo.Id.Should()
                .NotBeEmpty();
        }

        [Fact]

        public async Task GivenValidToken_Get_Returns_ListOfAccounts()
        {
            // Arrange
            Faker faker = new Faker();
            string password = faker.Lorem.Word();
            NewAccountInfo newAccount = new NewAccountInfo
            {
                Name = faker.Person.FullName,
                Username = faker.Person.UserName,
                Password = password,
                ConfirmPassword = password,
                Email = faker.Person.Email
            };

            _outputHelper.WriteLine($"Registering account {newAccount}");

            BearerTokenInfo bearerInfo = await _identityApiFixture.Register(newAccount)
                .ConfigureAwait(false);

            using HttpClient client = _identityApiFixture.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerInfo.AccessToken);

            // Act
            using HttpResponseMessage response = await client.GetAsync($"{_endpointUrl}/accounts?page=1")
                .ConfigureAwait(false);

            // Assert
            _outputHelper.WriteLine($"Response : {response}");
            _outputHelper.WriteLine($"Response's content : {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");

            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(Status200OK);

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
