namespace Measures.API.IntegrationTests.v1
{
    using Bogus;

    using FluentAssertions;

    using Identity.API.Fixtures.v2;

    using MedEasy.IntegrationTests.Core;
    using MedEasy.RestObjects;

    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Mvc;

    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Schema;

    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    using static Microsoft.AspNetCore.Http.StatusCodes;
    using static Newtonsoft.Json.JsonConvert;
    using static System.Net.Http.HttpMethod;

    /// <summary>
    /// Integration tests for endpoints related to <see cref="BloodPressureModel"/>
    /// </summary>
    [IntegrationTest]
    [Feature("Blood pressures")]
    [Feature("Measures")]
    public class BloodPressuresControllerTests : IAsyncLifetime, IClassFixture<IdentityApiFixture>, IClassFixture<IntegrationFixture<Startup>>
    {
        private readonly IntegrationFixture<Startup> _sut;
        private readonly IdentityApiFixture _identityServer;
        private readonly ITestOutputHelper _outputHelper;
        private const string _endpointUrl = "/v1/bloodpressures";
        private readonly Faker _faker;

        private static readonly JSchema _pageLink = new()
        {
            Type = JSchemaType.Object | JSchemaType.Null,
            Properties =
                {
                    [nameof(Link.Href).ToLower()] = new JSchema { Type = JSchemaType.String },
                    [nameof(Link.Relation).ToLower()] = new JSchema { Type = JSchemaType.String },
                    [nameof(Link.Method).ToLower()] = new JSchema { Type = JSchemaType.String },
                    [nameof(Link.Template).ToLower()] = new JSchema { Type = JSchemaType.Boolean | JSchemaType.Null },
                    [nameof(Link.Title).ToLower()] = new JSchema { Type = JSchemaType.Boolean | JSchemaType.Null }
                },
            Required = { nameof(Link.Href).ToLower(), nameof(Link.Relation).ToLower() },
            AllowAdditionalProperties = false
        };

        private static readonly JSchema _pageResponseSchema = new()
        {
            Type = JSchemaType.Object,
            Properties =
                {
                    [nameof(GenericPagedGetResponse<object>.Items).ToLower()] = new JSchema { Type = JSchemaType.Array},
                    [nameof(GenericPagedGetResponse<object>.Total).ToLower()] = new JSchema { Type = JSchemaType.Number, Minimum = 0 },
                    [nameof(GenericPagedGetResponse<object>.Links).ToLower()] = new JSchema
                    {
                        Type = JSchemaType.Object,
                        Properties =
                        {
                            [nameof(PageLinks.First).ToLower()] = _pageLink,
                            [nameof(PageLinks.Previous).ToLower()] = _pageLink,
                            [nameof(PageLinks.Next).ToLower()] = _pageLink,
                            [nameof(PageLinks.Last).ToLower()] = _pageLink
                        },
                        Required =
                        {
                            nameof(PageLinks.First).ToLower(),
                            nameof(PageLinks.Last).ToLower()
                        }
                    }
                },
            Required =
                {
                    nameof(GenericPagedGetResponse<object>.Items).ToLower(),
                    nameof(GenericPagedGetResponse<object>.Links).ToLower(),
                    nameof(GenericPagedGetResponse<object>.Total).ToLower()
                }
        };

        public BloodPressuresControllerTests(ITestOutputHelper outputHelper, IntegrationFixture<Startup> sut, IdentityApiFixture identityFixture)
        {
            _faker = new();
            _outputHelper = outputHelper;
            _sut = sut;
            _identityServer = identityFixture;
            _identityServer.Email = _faker.Person.Email;
            _identityServer.Password = _faker.Person.Email;
        }

        public async Task InitializeAsync() => await _identityServer.LogIn().ConfigureAwait(false);

        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task GetAll_With_No_Data()
        {
            // Arrange
            using HttpClient client = _sut.CreateClient();
            HttpRequestMessage getAllRequest = new(Get, _endpointUrl);
            getAllRequest.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, _identityServer.Tokens.AccessToken.Token);

            // Act
            using HttpResponseMessage response = await client.SendAsync(getAllRequest)
                                                             .ConfigureAwait(false);

            // Assert
            string json = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"json : {json}");

            ((int)response.StatusCode).Should().Be(Status200OK);
            
            JToken pageResponseToken = JToken.Parse(json);
            pageResponseToken.IsValid(_pageResponseSchema).Should()
                             .BeTrue();
        }

        public static IEnumerable<object[]> GetAll_With_Invalid_Pagination_Returns_BadRequestCases
        {
            get
            {
                int[] invalidPages = { int.MinValue, -1, -10, 0 };

                IEnumerable<(int page, int pageSize)> invalidCases = invalidPages.CrossJoin(invalidPages)
                    .Where(tuple => tuple.Item1 <= 0 || tuple.Item2 <= 0);

                foreach ((int page, int pageSize) in invalidCases)
                {
                    yield return new object[] { page, pageSize };
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetAll_With_Invalid_Pagination_Returns_BadRequestCases))]
        public async Task GetAll_With_Invalid_Pagination_Returns_BadRequest(int page, int pageSize)
        {
            _outputHelper.WriteLine($"Paging configuration : {SerializeObject(new { page, pageSize })}");

            // Arrange
            HttpRequestMessage getAllRequest = new(Head, $"{_endpointUrl}?page={page}&pageSize={pageSize}");
            getAllRequest.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, _identityServer.Tokens.AccessToken.Token);

            using HttpClient client = _sut.CreateClient();
            // Act
            HttpResponseMessage response = await client.SendAsync(getAllRequest)
                .ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should()
                .BeFalse("Invalid page and/or pageSize");
            ((int)response.StatusCode).Should().Be(Status400BadRequest);

            string content = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"Response content : {content}");

            content.Should()
                .NotBeNullOrEmpty();

            JToken validationProblemDetailsToken = JToken.Parse(content);
            //validationProblemDetailsToken.IsValid(_validationProblemDetailsSchema)
            //    .Should().BeTrue("Error object must be provided when API returns BAD REQUEST");

            ValidationProblemDetails errorObject = validationProblemDetailsToken.ToObject<ValidationProblemDetails>();
            errorObject.Status.Should()
                .Be(Status400BadRequest);
            errorObject.Errors.Should()
                .NotBeEmpty().And
                .ContainKeys("page", "pageSize");
        }

        //[Fact]
        //public async Task Enpoint_Provides_CountsHeaders()
        //{
        //    // Arrange
        //    const string password = "thecapedcrusader";
        //    Faker faker = new Faker();
        //    NewAccountInfo newAccountInfo = new NewAccountInfo
        //    {
        //        Name = faker.Person.FullName,
        //        Username = faker.Person.UserName,
        //        Password = password,
        //        ConfirmPassword = password,
        //        Email = faker.Person.Email
        //    };

        //    LoginInfo loginInfo = new LoginInfo
        //    {
        //        Username = newAccountInfo.Username,
        //        Password = newAccountInfo.Password
        //    };

        //    BearerTokenInfo bearerToken = await _identityServer.Register(newAccountInfo)
        //        .ConfigureAwait(false);

        //    string path = $"{_endpointUrl}";
        //    _outputHelper.WriteLine($"path under test : {path}");
        //    RequestBuilder requestBuilder = new RequestBuilder(_sut, path)
        //        .AddHeader("Authorization", $"Bearer {bearerToken.AccessToken}")
        //        ;

        //    // Act
        //    HttpResponseMessage response = await requestBuilder.SendAsync(Head)
        //        .ConfigureAwait(false);

        //    // Assert
        //    _outputHelper.WriteLine($"Response status code : {response.StatusCode}");
        //    response.IsSuccessStatusCode.Should().BeTrue();

        //    _outputHelper.WriteLine($"Response headers :{response.Headers.Stringify()}");

        //    response.Headers.Should()
        //        .ContainSingle(header => header.Key == AddCountHeadersFilterAttribute.TotalCountHeaderName).And
        //        .ContainSingle(header => header.Key == AddCountHeadersFilterAttribute.CountHeaderName);

        //    response.Headers.GetValues(AddCountHeadersFilterAttribute.TotalCountHeaderName).Should()
        //        .HaveCount(1).And
        //        .ContainSingle().And
        //        .ContainSingle(value => value == 0.ToString());

        //    response.Headers.GetValues(AddCountHeadersFilterAttribute.CountHeaderName).Should()
        //        .HaveCount(1).And
        //        .ContainSingle().And
        //        .ContainSingle(value => value == 0.ToString());

        //}

        //[Fact]
        //public async Task UnauthenticatedUser_Cant_Access_CountsHeader()
        //{
        //    // Arrange
        //    string path = $"{_endpointUrl}";
        //    _outputHelper.WriteLine($"path under test : {path}");
        //    RequestBuilder requestBuilder = _sut.CreateRequest(path);

        //    // Act
        //    HttpResponseMessage response = await requestBuilder.SendAsync(Head)
        //        .ConfigureAwait(false);

        //    // Assert
        //    _outputHelper.WriteLine($"Response status code : {response.StatusCode}");
        //    response.IsSuccessStatusCode.Should().BeFalse("the request must be authenticated using JWT token");
        //    response.StatusCode.Should()
        //        .Be(Status401Unauthorized);

        //}

        //[Theory]
        //[InlineData(_endpointUrl, "GET")]
        //[InlineData(_endpointUrl, "HEAD")]
        //[InlineData(_endpointUrl, "OPTIONS")]
        //public async Task ShouldReturnsSuccessCode(string url, string method)
        //{

        //    _outputHelper.WriteLine($"URL : <{url}>");
        //    _outputHelper.WriteLine($"method : <{method}>");

        //    // Arrange
        //    NewAccountInfo newAccountInfo = new NewAccountInfo
        //    {
        //        Username = "batman",
        //        Email = "batman@gotham.fr",
        //        Password = "thecapedcrusader",
        //        ConfirmPassword = "thecapedcrusader"
        //    };

        //    LoginInfo loginInfo = new LoginInfo
        //    {
        //        Username = newAccountInfo.Username,
        //        Password = newAccountInfo.Password
        //    };

        //    BearerTokenInfo bearerToken = await _identityServer.Register(newAccountInfo)
        //        .ConfigureAwait(false);
        //    RequestBuilder rb = _sut.CreateRequest(url)
        //        .AddHeader("Authorization", $"{JwtBearerDefaults.AuthenticationScheme} {bearerToken.AccessToken}");

        //    // Act
        //    HttpResponseMessage response = await rb.SendAsync(method)
        //        .ConfigureAwait(false);

        //    // Assert
        //    response.IsSuccessStatusCode.Should()
        //        .BeTrue($"'{method}' HTTP method must be supported");
        //    ((int)response.StatusCode).Should().Be(Status200OK);

        //}

        //[Theory]
        //[InlineData("HEAD")]
        //[InlineData("GET")]
        //[InlineData("DELETE")]
        //[InlineData("OPTIONS")]
        //public async Task Get_With_Empty_Id_Returns_Bad_Request(string method)
        //{
        //    _outputHelper.WriteLine($"method : <{method}>");

        //    // Arrange
        //    NewAccountInfo newAccountInfo = new NewAccountInfo
        //    {
        //        Username = "batman",
        //        Email = "batman@gotham.fr",
        //        Password = "thecapedcrusader",
        //        ConfirmPassword = "thecapedcrusader"
        //    };

        //    LoginInfo loginInfo = new LoginInfo
        //    {
        //        Username = newAccountInfo.Username,
        //        Password = newAccountInfo.Password
        //    };

        //    BearerTokenInfo bearerToken = await _identityServer.Register(newAccountInfo)
        //        .ConfigureAwait(false);
        //    string url = $"{_endpointUrl}/{Guid.Empty.ToString()}";
        //    _outputHelper.WriteLine($"Requested url : <{url}>");

        //    RequestBuilder requestBuilder = new RequestBuilder(_sut, url)
        //        .AddHeader("Accept", "application/json")
        //        .AddHeader("Authorization", $"{JwtBearerDefaults.AuthenticationScheme} {bearerToken.AccessToken}");

        //    // Act
        //    HttpResponseMessage response = await requestBuilder.SendAsync(method)
        //        .ConfigureAwait(false);

        //    // Assert
        //    response.IsSuccessStatusCode.Should()
        //        .BeFalse("the requested bloodpressure id is empty");
        //    ((int)response.StatusCode).Should()
        //        .Be(Status400BadRequest, "the requested bloodpressure id is empty");

        //    ((int)response.StatusCode).Should().Be(Status400BadRequest, "the requested bloodpressure id is not empty and it's part of the url");

        //    if (IsGet(method))
        //    {
        //        string content = await response.Content.ReadAsStringAsync()
        //                .ConfigureAwait(false);

        //        _outputHelper.WriteLine($"Bad request content : {content}");

        //        content.Should()
        //            .NotBeNullOrEmpty();

        //        JToken errorToken = JToken.Parse(content);
        //        errorToken.IsValid(_errorObjectSchema)
        //            .Should().BeTrue("Error object must be provided when API returns BAD REQUEST");

        //        ErrorObject errorObject = errorToken.ToObject<ErrorObject>();
        //        errorObject.Code.Should()
        //            .Be("BAD_REQUEST");
        //        errorObject.Description.Should()
        //            .Be("Validation failed");
        //        errorObject.Errors.Should()
        //            .HaveCount(1).And
        //            .ContainKey("id").WhichValue.Should()
        //                .HaveCount(1).And
        //                .HaveElementAt(0, $"'id' must have a non default value");
        //    }

        //}

        //[Fact]
        //public async Task Patch_With_EmptyId_Returns_Bad_Request()
        //{
        //    // Arrange
        //    JsonPatchDocument<BloodPressureInfo> changes = new JsonPatchDocument<BloodPressureInfo>();
        //    changes.Replace(x => x.SystolicPressure, 120);

        //    NewAccountInfo newAccountInfo = new NewAccountInfo
        //    {
        //        Username = "batman",
        //        Email = "batman@gotham.fr",
        //        Password = "thecapedcrusader",
        //        ConfirmPassword = "thecapedcrusader"
        //    };

        //    LoginInfo loginInfo = new LoginInfo
        //    {
        //        Username = newAccountInfo.Username,
        //        Password = newAccountInfo.Password
        //    };

        //    BearerTokenInfo bearerToken = await _identityServer.Register(newAccountInfo)
        //        .ConfigureAwait(false);
        //    RequestBuilder requestBuilder = new RequestBuilder(_sut, $"{_endpointUrl}/{Guid.Empty}")
        //        .AddHeader("Accept", "application/json")
        //        .AddHeader("Authorization", $"{JwtBearerDefaults.AuthenticationScheme} {bearerToken.AccessToken}")
        //        .And(request => request.Content = new StringContent(changes.ToString(), Encoding.UTF8, "application/json-patch+json"));

        //    // Act
        //    HttpResponseMessage response = await requestBuilder.SendAsync(Patch)
        //        .ConfigureAwait(false);

        //    // Assert
        //    response.IsSuccessStatusCode.Should()
        //        .BeFalse();
        //    ((int)response.StatusCode).Should()
        //        .Be(Status400BadRequest);

        //    string json = await response.Content.ReadAsStringAsync()
        //        .ConfigureAwait(false);

        //    _outputHelper.WriteLine($"Bad request content : {json}");

        //    json.Should()
        //        .NotBeNullOrEmpty("A body describing the error must be provided");

        //    JToken errorToken = JToken.Parse(json);

        //    errorToken.IsValid(_errorObjectSchema).Should()
        //        .BeTrue();

        //    ErrorObject errorObject = errorToken.ToObject<ErrorObject>();
        //    errorObject.Code.Should()
        //        .Be("BAD_REQUEST");
        //    errorObject.Description.Should()
        //        .Be("Validation failed");
        //    errorObject.Errors.Should()
        //        .HaveCount(1).And
        //        .ContainKey("id").WhichValue.Should()
        //            .HaveCount(1).And
        //            .HaveElementAt(0, "'id' must have a non default value");
        //}

        //[Theory]
        //[InlineData("HEAD", "")]
        //[InlineData("GET", "")]
        //public async Task GivenInvalidQueryString_CallingSearch_ShouldReturns_BadRequest(string method, string queryString)
        //{
        //    // Arrange
        //    string url = $"{_endpointUrl}/search{queryString}";
        //    NewAccountInfo newAccountInfo = new NewAccountInfo
        //    {
        //        Username = "batman",
        //        Email = "batman@gotham.fr",
        //        Password = "thecapedcrusader",
        //        ConfirmPassword = "thecapedcrusader"
        //    };

        //    LoginInfo loginInfo = new LoginInfo
        //    {
        //        Username = newAccountInfo.Username,
        //        Password = newAccountInfo.Password
        //    };

        //    BearerTokenInfo bearerToken = await _identityServer.Register(newAccountInfo)
        //        .ConfigureAwait(false);
        //    _outputHelper.WriteLine($"URL : {url}");
        //    _outputHelper.WriteLine($"Method : {method}");

        //    RequestBuilder requestBuilder = new RequestBuilder(_sut, url)
        //        .AddHeader("Authorization", $"{JwtBearerDefaults.AuthenticationScheme} {bearerToken.AccessToken}");

        //    // Act
        //    HttpResponseMessage response = await requestBuilder.SendAsync(method)
        //        .ConfigureAwait(false);

        //    // Assert
        //    response.IsSuccessStatusCode.Should().BeFalse($"a non empty query string must be provided to the <{url}>");
        //    ((int)response.StatusCode).Should().Be(Status400BadRequest);
        //}

    }
}
