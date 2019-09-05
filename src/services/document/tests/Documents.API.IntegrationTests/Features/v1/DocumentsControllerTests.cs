using Bogus;
using Documents.DTO;
using Documents.DTO.v1;
using FluentAssertions;
using Identity.DTO;
using Identity.DTO.v2;
using MedEasy.IntegrationTests.Core;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Newtonsoft.Json.JsonConvert;
using static System.Net.Http.HttpMethod;
using IdentityApiFixture = Identity.API.Fixtures.v2.IdentityApiFixture;

namespace Documents.API.IntegrationTests.Features.v1
{
    [IntegrationTest]
    [Feature(nameof(Documents))]
    public class DocumentsControllerTests : IDisposable, IClassFixture<IdentityApiFixture>, IClassFixture<IntegrationFixture<Startup>>
    {
        private IdentityApiFixture _identityApiFixture;
        private ITestOutputHelper _outputHelper;
        private const string _endpointUrl = "/v1/documents/";
        private readonly IntegrationFixture<Startup> _sut;

        public DocumentsControllerTests(ITestOutputHelper outputHelper, IdentityApiFixture identityFixture, IntegrationFixture<Startup> sut)
        {
            _outputHelper = outputHelper;
            _identityApiFixture = identityFixture;
            _sut = sut;
        }

        public void Dispose()
        {
            _outputHelper = null;

            _identityApiFixture = null;
        }

        [Fact]
        public async Task GivenNoToken_HeadAll_Returns_Unauthorized()
        {
            // Arrange

            string requestUri = $"{_endpointUrl}?page=1&pageSize=5";
            _outputHelper.WriteLine($"Requested URI : {requestUri}");
            HttpRequestMessage headRequest = new HttpRequestMessage(Head, requestUri);
            using HttpClient client = _sut.CreateClient();

            // Act
            using HttpResponseMessage response = await client.SendAsync(headRequest)
                .ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should()
                .BeFalse("no bearer token was provided");

            response.StatusCode.Should()
                .Be(Status401Unauthorized);
        }

        [Fact]
        public async Task GivenDocument_Post_Creates_Record()
        {
            // Arrange
            Faker faker = new Faker();
            string password = faker.Lorem.Word();

            NewDocumentInfo newResourceInfo = new NewDocumentInfo
            {
                Name = faker.System.CommonFileName(),
                MimeType = faker.System.CommonFileType(),
                Content = faker.Hacker.Random.Bytes(20),
            };

            NewAccountInfo newAccountInfo = new NewAccountInfo
            {
                Name = faker.Person.FullName,
                Email = faker.Person.Email,
                Password = password,
                ConfirmPassword = password,
                Username = faker.Person.UserName
            };

            BearerTokenInfo tokenInfo = await _identityApiFixture.Register(newAccountInfo)
                .ConfigureAwait(false);

            HttpContent content = new StringContent(SerializeObject(newResourceInfo), Encoding.Default, "application/json");

            using HttpClient client = _sut.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, tokenInfo.AccessToken.Token);

            // Act
            using HttpResponseMessage response = await client.PostAsync(_endpointUrl, content)
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
            JSchema responseSchema = new JSchemaGenerator()
                .Generate(typeof(Browsable<DocumentInfo>));
            jsonToken.IsValid(responseSchema);

            Browsable<DocumentInfo> browsableResource = jsonToken.ToObject<Browsable<DocumentInfo>>();

            IEnumerable<Link> links = browsableResource.Links;
            links.Should()
                .NotBeEmpty().And
                .NotContainNulls().And
                .NotContain(link => string.IsNullOrWhiteSpace(link.Href), $"{nameof(Link.Href)} must be explicitely specified for each link").And
                .NotContain(link => string.IsNullOrWhiteSpace(link.Relation), $"{nameof(Link.Relation)} must be explicitely specified for each link").And
                .NotContain(link => string.IsNullOrWhiteSpace(link.Method), $"{nameof(Link.Method)} must be explicitely specified for each link").And
                .Contain(link => link.Relation == LinkRelation.Self, $"a direct link to the resource must be provided");

            Link linkToSelf = links.Single(link => link.Relation == LinkRelation.Self);
            linkToSelf.Method.Should()
                .Be("GET");

            DocumentInfo createdResourceInfo = browsableResource.Resource;

            createdResourceInfo.Name.Should()
                .Be(newResourceInfo.Name);
            createdResourceInfo.MimeType.Should()
                .Be(newResourceInfo.MimeType);
            createdResourceInfo.Hash.Should()
                .NotBeNullOrWhiteSpace();

            createdResourceInfo.Id.Should()
                .NotBeEmpty();
        }

        [Fact]

        public async Task GivenValidToken_Get_Returns_ListOfDocuments()
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

            _outputHelper.WriteLine($"Registering account {newAccount.Stringify()}");

            BearerTokenInfo bearerInfo = await _identityApiFixture.Register(newAccount)
                .ConfigureAwait(false);

            using HttpClient client = _sut.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerInfo.AccessToken.Token);

            // Act
            using HttpResponseMessage response = await client.GetAsync($"{_endpointUrl}?page=1")
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