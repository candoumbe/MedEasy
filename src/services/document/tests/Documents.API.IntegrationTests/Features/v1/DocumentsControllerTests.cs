namespace Documents.API.IntegrationTests.Features.v1
{
    using Bogus;

    using Documents.DTO;
    using Documents.DTO.v1;
    using Documents.Ids;

    using FluentAssertions;

    using Identity.API.Fixtures.v2;
    using Identity.DTO;
    using Identity.Ids;

    using MedEasy.IntegrationTests.Core;
    using MedEasy.RestObjects;

    using Microsoft.AspNetCore.Authentication.JwtBearer;

    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Schema;
    using Newtonsoft.Json.Schema.Generation;

    using NodaTime;
    using NodaTime.Serialization.SystemTextJson;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;
    using Xunit.Extensions.AssemblyFixture;

    using static Microsoft.AspNetCore.Http.StatusCodes;
    using static System.Net.Http.HttpMethod;

    [IntegrationTest]
    [Feature(nameof(Documents))]
    public class DocumentsControllerTests : IAsyncLifetime, IAssemblyFixture<IdentityApiFixture>, IAssemblyFixture<IntegrationFixture<Startup>>
    {
        private readonly IdentityApiFixture _identityApiFixture;
        private readonly ITestOutputHelper _outputHelper;
        private const string _endpointUrl = "/v1/documents/";
        private readonly IntegrationFixture<Startup> _sut;
        private readonly Faker _faker;

        private JsonSerializerOptions SerializerOptions
        {
            get
            {
                JsonSerializerOptions options = new JsonSerializerOptions().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                options.PropertyNameCaseInsensitive = true;
                return options;
            }
        }

        public DocumentsControllerTests(ITestOutputHelper outputHelper, IdentityApiFixture identityFixture, IntegrationFixture<Startup> sut)
        {
            _faker = new();
            _outputHelper = outputHelper;
            _identityApiFixture = identityFixture;
            _sut = sut;
        }

        ///<inheritdoc/>
        public async Task InitializeAsync() => await _identityApiFixture.LogIn().ConfigureAwait(false);

        ///<inheritdoc/>
        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task GivenDocument_Post_Creates_Record()
        {
            // Arrange
            Faker faker = new();
            string password = faker.Lorem.Word();

            NewDocumentInfo newResourceInfo = new()
            {
                Name = faker.System.CommonFileName(),
                MimeType = faker.System.CommonFileType(),
                Content = faker.Hacker.Random.Bytes(20),
            };

            using HttpClient client = _sut.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, _identityApiFixture.Tokens.AccessToken.Token);

            // Act
            using HttpResponseMessage response = await client.PostAsJsonAsync(_endpointUrl, newResourceInfo, SerializerOptions)
                                                            .ConfigureAwait(false);

            // Assert
            _outputHelper.WriteLine($"Response : {response}");
            response.IsSuccessStatusCode.Should()
                                        .BeTrue();
            response.StatusCode.Should()
                               .Be(Status201Created);

            string jsonResponse = await response.Content.ReadAsStringAsync()
                                                        .ConfigureAwait(false);

            JObject jsonToken = JObject.Parse(jsonResponse);
            JSchema responseSchema = new JSchemaGenerator().Generate(typeof(Browsable<DocumentInfo>));
            jsonToken.IsValid(responseSchema);

            Browsable<DocumentInfo> browsableDocument = await response.Content.ReadFromJsonAsync<Browsable<DocumentInfo>>(SerializerOptions)
                                                                              .ConfigureAwait(false);
            IEnumerable<Link> links = browsableDocument.Links;
            links.Should()
                .NotBeEmpty().And
                .NotContainNulls().And
                .NotContain(link => string.IsNullOrWhiteSpace(link.Href), $"{nameof(Link.Href)} must be explicitely specified for each link").And
                .NotContain(link => string.IsNullOrWhiteSpace(link.Relation), $"{nameof(Link.Relation)} must be explicitely specified for each link").And
                .NotContain(link => string.IsNullOrWhiteSpace(link.Method), $"{nameof(Link.Method)} must be explicitely specified for each link").And
                .Contain(link => link.Relation == LinkRelation.Self, "a direct link to the resource must be provided");

            Link linkToSelf = links.Single(link => link.Relation == LinkRelation.Self);
            linkToSelf.Method.Should()
                             .Be("GET");

            DocumentInfo document = browsableDocument.Resource;
            document.Name.Should()
                         .Be(newResourceInfo.Name);
            document.MimeType.Should()
                             .Be(newResourceInfo.MimeType);
            document.Hash.Should()
                         .NotBeNullOrWhiteSpace();
            document.Id.Should()
                       .NotBeNull().And
                       .NotBe(DocumentId.Empty);
        }

        [Fact]
        public async Task GivenValidToken_Get_Returns_ListOfDocuments()
        {
            // Arrange

            _outputHelper.WriteLine($"Bearer : {_identityApiFixture.Tokens.AccessToken.Jsonify()}");

            using HttpClient client = _sut.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _identityApiFixture.Tokens.AccessToken.Token);

            // Act
            using HttpResponseMessage response = await client.GetAsync($"{_endpointUrl}?page=1")
                                                             .ConfigureAwait(false);

            // Assert
            _outputHelper.WriteLine($"Response : {response}");
            _outputHelper.WriteLine($"Response's content : {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");

            response.IsSuccessStatusCode.Should()
                                        .BeTrue();
            response.StatusCode.Should().Be(Status200OK);

            string jsonResponse = await response.Content.ReadAsStringAsync()
                                                        .ConfigureAwait(false);

            JToken jsonToken = JToken.Parse(jsonResponse);
            JSchema responseSchema = new JSchemaGenerator().Generate(typeof(GenericPagedGetResponse<Browsable<AccountInfo>>));
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
        //    {>
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