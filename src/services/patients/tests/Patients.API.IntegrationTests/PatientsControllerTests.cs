namespace Patients.API.IntegrationTests
{
    using Bogus;

    using FluentAssertions;

    using Identity.API.Fixtures.v2;

    using MedEasy.IntegrationTests.Core;
    using MedEasy.RestObjects;

    using Microsoft.AspNetCore.Authentication.JwtBearer;

    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Schema;

    using Patients.DTO;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;
    using Xunit.Extensions.AssemblyFixture;

    using static Microsoft.AspNetCore.Http.StatusCodes;
    using static System.Net.Http.HttpMethod;

    [IntegrationTest]
    [Feature("Patients")]
    public class PatientsControllerTests : IAsyncLifetime, IAssemblyFixture<IntegrationFixture<Startup>>, IAssemblyFixture<IdentityApiFixture>
    {
        private readonly IntegrationFixture<Startup> _server;
        private readonly ITestOutputHelper _outputHelper;
        private readonly IdentityApiFixture _identityServer;
        private const string _endpointUrl = "/patients";
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

        public PatientsControllerTests(ITestOutputHelper outputHelper, IntegrationFixture<Startup> fixture, IdentityApiFixture identityFixture)
        {
            _faker = new();
            _outputHelper = outputHelper;
            _server = fixture;
            _identityServer = identityFixture;
        }

        public async Task InitializeAsync()
        {
            await _identityServer.LogIn().ConfigureAwait(false);
        }

        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task GetAll_With_No_Data()
        {
            // Arrange
            using HttpClient client = _server.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, _identityServer.Tokens.AccessToken.Token);

            // Act
            using HttpResponseMessage response = await client.GetAsync("/patients")
                                                       .ConfigureAwait(false);

            // Assert
            string json = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"json : {json}");

            ((int)response.StatusCode).Should().Be(Status200OK);
            JToken jToken = JToken.Parse(json);
            jToken.IsValid(_pageResponseSchema).Should().BeTrue();
        }

        public static IEnumerable<object[]> ShouldReturnsSucessCodeCases
        {
            get
            {
                const string url = "/patients";
                yield return new object[] { url, Head };
                yield return new object[] { url, Get };
                yield return new object[] { url, Options };
            }
        }

        [Theory]
        [MemberData(nameof(ShouldReturnsSucessCodeCases))]
        public async Task ShouldReturnsSuccessCode(string url, HttpMethod method)
        {
            _outputHelper.WriteLine($"Perfoming HTTP <{method}> on <{url}>");

            // Arrange
            using HttpClient client = _server.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, _identityServer.Tokens.AccessToken.Token);
            HttpRequestMessage message = new(method, url);

            // Act
            using HttpResponseMessage response = await client.SendAsync(message, default).ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should()
                                        .BeTrue($"'{method}' HTTP method must be supported");
            ((int)response.StatusCode).Should().Be(Status200OK);
        }

        public static IEnumerable<object[]> RequestWithEmptyIdReturnsBadRequestCases
        {
            get
            {
                yield return new object[] { Head };
                yield return new object[] { Get };
                yield return new object[] { Delete };
                yield return new object[] { Options };
            }
        }

        [Theory]
        [MemberData(nameof(RequestWithEmptyIdReturnsBadRequestCases))]
        public async Task Given_method_is_supported_when_id_is_empty_response_should_be_BadRequest(HttpMethod method)
        {
            _outputHelper.WriteLine($"method : <{method}>");

            // Arrange
            string url = $"{_endpointUrl}/{Guid.Empty}";
            _outputHelper.WriteLine($"Requested url : <{url}>");

            using HttpClient client = _server.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, _identityServer.Tokens.AccessToken.Token);
            using HttpRequestMessage message = new(method, url);

            // Act
            using HttpResponseMessage response = await client.SendAsync(message)
                                                             .ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should()
                .BeFalse("the requested patient id is empty");
            ((int)response.StatusCode).Should().Be(Status400BadRequest, "the requested patient id must not be empty and it's part of the url");
        }

        [Fact]
        public async Task GivenEmptyEndpoint_GetPageTwoOfEmptyResult_Returns_NotFound()
        {
            // Arrange
            const string url = $"{_endpointUrl}/search?page=2&pageSize=10&firstname=Bruce";
            using HttpClient client = _server.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, _identityServer.Tokens.AccessToken.Token);

            // Act
            using HttpResponseMessage response = await client.GetAsync(url)
                                                             .ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should()
                                        .BeFalse("The page of results doesn't exist");

            _outputHelper.WriteLine($"response : {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");

            ((int)response.StatusCode).Should()
                                      .Be(Status404NotFound);
        }

        [Fact]
        public async Task Create_Resource()
        {
            // Arrange
            _outputHelper.WriteLine($"Token : {_identityServer.Tokens.AccessToken}");

            using HttpClient client = _server.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, _identityServer.Tokens.AccessToken.Token);

            CreatePatientInfo newPatient = new()
            {
                Firstname = "Victor",
                Lastname = "Freeze"
            };

            // Act
            using HttpResponseMessage response = await client.PostAsJsonAsync("/patients", newPatient, default)
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"HTTP create patient status code : {response.StatusCode}");

            //Assert
            Uri location = response.Headers.Location;
            _outputHelper.WriteLine($"Location of the resource : <{location}>");
            location.Should().NotBeNull();
            location.IsAbsoluteUri.Should().BeTrue("location of the resource must be an absolute URI");
            HttpRequestMessage headMessage = new(Head, location);
            using HttpResponseMessage checkResponse = await client.SendAsync(headMessage)
                                                                  .ConfigureAwait(false);

            checkResponse.IsSuccessStatusCode.Should()
                                             .BeTrue($"The content location must point to the created resource");
        }

        [Fact]
        public async Task GivenPatientExists_AllLinksWithGetMethod_ShouldBe_Valid()
        {
            // Arrange
            CreatePatientInfo newPatient = new()
            {
                Firstname = "Victor",
                Lastname = "Freeze"
            };

            using HttpClient client = _server.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, _identityServer.Tokens.AccessToken.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync("/patients", newPatient, default)
                                                       .ConfigureAwait(false);

            _outputHelper.WriteLine($"HTTP create patient status code : {response.StatusCode}");

            string json = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            _outputHelper.WriteLine($"json : {json}");
            IEnumerable<Link> patientLinks = JToken.Parse(json)[nameof(Browsable<PatientInfo>.Links).ToLower()].ToObject<IEnumerable<Link>>();
            IEnumerable<Link> linksToGetData = patientLinks.Where(x => x.Method == "GET");

            using HttpClient client2 = _server.CreateClient(new()
            {
                BaseAddress = client.BaseAddress
            });
            foreach (Link link in linksToGetData)
            {
                HttpRequestMessage headRequestMessage = new()
                {
                    Method = Head,
                    RequestUri = new Uri(link.Href, UriKind.Relative)
                };
                headRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, _identityServer.Tokens.AccessToken.Token);

                // Act
                response = await client2.SendAsync(headRequestMessage)
                                        .ConfigureAwait(false);

                // Assert
                _outputHelper.WriteLine($"HTTP HEAD <{link.Href}> status code : <{response.StatusCode}>");
                response.IsSuccessStatusCode.Should()
                    .BeTrue($"<{link.Href}> should be accessible as it was returned as part of the response after creating patient resource");
            }
        }
    }
}
