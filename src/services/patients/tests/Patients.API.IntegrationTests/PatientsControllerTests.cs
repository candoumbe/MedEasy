using FluentAssertions;
using FluentAssertions.Extensions;
using Identity.API.Fixtures.v2;
using Identity.DTO;
using Identity.DTO.v2;
using Patients.API;
using Patients.DTO;
using MedEasy.IntegrationTests.Core;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
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

namespace Patients.API.IntegrationTests
{
    [IntegrationTest]
    [Feature("Patients")]
    public class PatientsControllerTests : IDisposable, IClassFixture<IntegrationFixture<Startup>>, IClassFixture<IdentityApiFixture>
    {
        private IntegrationFixture<Startup> _server;
        private ITestOutputHelper _outputHelper;
        private IdentityApiFixture _identityServer;
        private const string _endpointUrl = "/patients";

        private static readonly JSchema _errorObjectSchema = new JSchema
        {
            Type = JSchemaType.Object,
            Properties =
            {
                [nameof(ValidationProblemDetails.Title).ToLower()] = new JSchema { Type = JSchemaType.String},
                [nameof(ValidationProblemDetails.Status).ToLower()] = new JSchema { Type = JSchemaType.Number},
                [nameof(ValidationProblemDetails.Detail).ToLower()] = new JSchema { Type = JSchemaType.String },
                [nameof(ValidationProblemDetails.Errors).ToLower()] = new JSchema { Type = JSchemaType.Object },
            },
            Required =
            {
                nameof(ValidationProblemDetails.Title).ToLower(),
                nameof(ValidationProblemDetails.Status).ToLower(),

            }
        };

        private static readonly JSchema _pageLink = new JSchema
        {
            Type = JSchemaType.Object,
            Properties =
                {
                    [nameof(Link.Href).ToLower()] = new JSchema { Type = JSchemaType.String },
                    [nameof(Link.Relation).ToLower()] = new JSchema { Type = JSchemaType.String },
                    [nameof(Link.Method).ToLower()] = new JSchema { Type = JSchemaType.String }
                },
            Required = { nameof(Link.Href).ToLower(), nameof(Link.Relation).ToLower() },
            AllowAdditionalProperties = false
        };

        private static readonly JSchema _pageResponseSchema = new JSchema
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
            _outputHelper = outputHelper;
            _server = fixture;
            _identityServer = identityFixture;
        }

        public void Dispose()
        {
            _outputHelper = null;
            _server = null;
            _identityServer = null;
        }

        [Fact]
        public async Task GetAll_With_No_Data()
        {
            // Arrange
            NewAccountInfo newAccountInfo = new NewAccountInfo
            {
                Username = $"robin_{Guid.NewGuid()}",
                Email = $"batman_{Guid.NewGuid()}@gotham.fr",
                Password = "thecapedcrusader",
                ConfirmPassword = "thecapedcrusader"
            };

            BearerTokenInfo bearerToken = await _identityServer.RegisterAndLogIn(newAccountInfo)
                .ConfigureAwait(false);

            LoginInfo loginInfo = new LoginInfo
            {
                Username = newAccountInfo.Username,
                Password = newAccountInfo.Password
            };

            using HttpClient client = _server.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, bearerToken.AccessToken.Token);

            // Act
            HttpResponseMessage response = await client.GetAsync("/patients")
                .ConfigureAwait(false);

            // Assert
            string json = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"json : {json}");

            ((int)response.StatusCode).Should().Be(Status200OK);
            HttpContentHeaders headers = response.Content.Headers;


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
            NewAccountInfo newAccountInfo = new NewAccountInfo
            {
                Username = $"dick_{Guid.NewGuid()}",
                Email = $"batman_{Guid.NewGuid().ToString("N")}@gotham.fr",
                Password = "thecapedcrusader",
                ConfirmPassword = "thecapedcrusader"
            };

            LoginInfo loginInfo = new LoginInfo
            {
                Username = newAccountInfo.Username,
                Password = newAccountInfo.Password
            };

            BearerTokenInfo bearerToken = await _identityServer.RegisterAndLogIn(newAccountInfo)
                .ConfigureAwait(false);
            using (HttpClient client = _server.CreateClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, bearerToken.AccessToken.Token);
                HttpRequestMessage message = new HttpRequestMessage(method, url);

                // Act
                HttpResponseMessage response = await client.SendAsync(message)
                    .ConfigureAwait(false);

                // Assert
                response.IsSuccessStatusCode.Should()
                    .BeTrue($"'{method}' HTTP method must be supported");
                ((int)response.StatusCode).Should().Be(Status200OK);
            }
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
        public async Task Get_With_Empty_Id_Returns_Bad_Request(HttpMethod method)
        {
            _outputHelper.WriteLine($"method : <{method}>");

            // Arrange
            string url = $"{_endpointUrl}/{Guid.Empty}";
            _outputHelper.WriteLine($"Requested url : <{url}>");

            NewAccountInfo newAccountInfo = new NewAccountInfo
            {
                Username = $"robin_{Guid.NewGuid()}",
                Email = $"batman_{Guid.NewGuid()}@gotham.fr",
                Password = "thecapedcrusader",
                ConfirmPassword = "thecapedcrusader"
            };

            BearerTokenInfo bearerToken = await _identityServer.RegisterAndLogIn(newAccountInfo)
                .ConfigureAwait(false);

            using HttpClient client = _server.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, bearerToken.AccessToken.Token);
            HttpRequestMessage message = new HttpRequestMessage(method, url);

            // Act
            HttpResponseMessage response = await client.SendAsync(message)
                .ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should()
                .BeFalse("the requested patient id is empty");
            ((int)response.StatusCode).Should()
                .Be(Status400BadRequest, "the requested patient id is empty");

            ((int)response.StatusCode).Should().Be(Status400BadRequest, "the requested patient id must not be empty and it's part of the url");

            if (method == Get)
            {
                string content = await response.Content.ReadAsStringAsync()
                        .ConfigureAwait(false);

                _outputHelper.WriteLine($"Bad request content : {content}");

                content.Should()
                    .NotBeNullOrEmpty();

                JToken token = JToken.Parse(content);
                token.IsValid(_errorObjectSchema)

                    .Should().BeTrue("Error object must be provided when API returns BAD REQUEST");

                ValidationProblemDetails errorObject = token.ToObject<ValidationProblemDetails>();
                errorObject.Title.Should()
                    .Be("Validation failed");
                errorObject.Errors.Should()
                    .HaveCount(1).And
                    .ContainKey("id").WhichValue.Should()
                        .HaveCount(1).And
                        .HaveElementAt(0, "'id' must have a non default value");
            }
        }

        [Fact]
        public async Task GivenEmptyEndpoint_GetPageTwoOfEmptyResult_Returns_NotFound()
        {
            // Arrange
            string url = $"{_endpointUrl}/search?page=2&page10&firstname=Bruce";
            _outputHelper.WriteLine($"Requested url : <{url}>");

            NewAccountInfo newAccountInfo = new NewAccountInfo
            {
                Username = $"batman_{Guid.NewGuid()}",
                Email = $"batman_{Guid.NewGuid()}@gotham.fr",
                Password = "thecapedcrusader",
                ConfirmPassword = "thecapedcrusader"
            };

            LoginInfo loginInfo = new LoginInfo
            {
                Username = newAccountInfo.Username,
                Password = newAccountInfo.Password
            };

            BearerTokenInfo bearerToken = await _identityServer.RegisterAndLogIn(newAccountInfo)
                .ConfigureAwait(false);
            using (HttpClient client = _server.CreateClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, bearerToken.AccessToken.Token);

                // Act
                HttpResponseMessage response = await client.GetAsync(url)
                    .ConfigureAwait(false);

                // Assert
                response.IsSuccessStatusCode.Should()
                    .BeFalse("The page of results doesn't exist");
                ((int)response.StatusCode).Should()
                    .Be(Status404NotFound);
            }
        }

        [Fact]
        public async Task Create_Resource()
        {
            // Arrange
            NewAccountInfo newAccountInfo = new NewAccountInfo
            {
                Username = $"robin_{Guid.NewGuid()}",
                Email = "robin@gotham.fr",
                Password = "thecapedcrusader",
                ConfirmPassword = "thecapedcrusader"
            };

            BearerTokenInfo bearerToken = await _identityServer.RegisterAndLogIn(newAccountInfo)
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"Token : {bearerToken}");

            using HttpClient client = _server.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, bearerToken.AccessToken.Token);

            CreatePatientInfo newPatient = new CreatePatientInfo
            {
                Firstname = "Victor",
                Lastname = "Freeze"
            };

            // Act
            HttpResponseMessage response = await client.PostAsJsonAsync("/patients", newPatient)
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"HTTP create patient status code : {response.StatusCode}");

            string json = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"created resource : {json}");

            string patientId = JToken.Parse(json)[nameof(Browsable<PatientInfo>.Resource).ToLower()][nameof(PatientInfo.Id).ToLower()].ToString();

            //Assert
            Uri location = response.Headers.Location;
            _outputHelper.WriteLine($"Location of the resource : <{location}>");
            location.Should().NotBeNull();
            location.IsAbsoluteUri.Should().BeTrue("location of the resource must be an absolute URI");
            HttpRequestMessage headMessage = new HttpRequestMessage(Head, location);
            HttpResponseMessage checkResponse = await client.SendAsync(headMessage)
                .ConfigureAwait(false);

            checkResponse.IsSuccessStatusCode.Should().BeTrue($"The content location must point to the created resource");
        }

        
        [Fact]
        public async Task GivenPatientExists_AllLinksWithGetMethod_ShouldBe_Valid()
        {
            // Arrange
            CreatePatientInfo newPatient = new CreatePatientInfo
            {
                Firstname = "Victor",
                Lastname = "Freeze"
            };

            string username = $"batman_{Guid.NewGuid()}";
            NewAccountInfo newAccountInfo = new NewAccountInfo
            {
                Username = username,
                Email = $"{username}@gotham.fr",
                Password = "thecapedcrusader",
                ConfirmPassword = "thecapedcrusader"
            };

            BearerTokenInfo bearerToken = await _identityServer.RegisterAndLogIn(newAccountInfo)
                .ConfigureAwait(false);

            LoginInfo loginInfo = new LoginInfo
            {
                Username = newAccountInfo.Username,
                Password = newAccountInfo.Password
            };

            using (HttpClient client = _server.CreateClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, bearerToken.AccessToken.Token);

                HttpResponseMessage response = await client.PostAsJsonAsync("/patients", newPatient)
                    .ConfigureAwait(false);

                _outputHelper.WriteLine($"HTTP create patient status code : {response.StatusCode}");

                string json = await response.Content.ReadAsStringAsync()
                    .ConfigureAwait(false);
                _outputHelper.WriteLine($"json : {json}");
                IEnumerable<Link> patientLinks = JToken.Parse(json)[nameof(Browsable<PatientInfo>.Links).ToLower()].ToObject<IEnumerable<Link>>();
                IEnumerable<Link> linksToGetData = patientLinks.Where(x => x.Method == "GET");

                using (HttpClient client2 = _server.CreateClient())
                {
                    foreach (Link link in linksToGetData)
                    {
                        HttpRequestMessage headRequestMessage = new HttpRequestMessage
                        {
                            Method = Head,
                            RequestUri = new Uri(link.Href)
                        };
                        headRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, bearerToken.AccessToken.Token);

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
    }
}
