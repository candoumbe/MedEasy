using FluentAssertions;
using FluentAssertions.Extensions;

using Forms;

using Identity.API.Fixtures.v2;
using Identity.DTO;
using Identity.DTO.v2;

using Measures.API.Features.Patients;
using Measures.DTO;

using MedEasy.IntegrationTests.Core;
using MedEasy.Models;
using MedEasy.RestObjects;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Superpower.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

using static Microsoft.AspNetCore.Http.StatusCodes;
using static System.Net.Http.HttpMethod;

namespace Measures.API.IntegrationTests.v1
{
    [IntegrationTest]
    [Feature("Patients")]
    public class PatientsControllerTests : IClassFixture<IntegrationFixture<Startup>>, IClassFixture<IdentityApiFixture>
    {
        private readonly IntegrationFixture<Startup> _server;
        private readonly ITestOutputHelper _outputHelper;
        private readonly IdentityApiFixture _identityServer;
        private const string _version = "v1";
        private readonly static string _baseUrl = $"/{_version}/patients";

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
                    [nameof(Link.Template).ToLower()] = new JSchema { Type = JSchemaType.Boolean } ,
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
                    [nameof(GenericPageModel<object>.Items).ToLower()] = new JSchema { Type = JSchemaType.Array},
                    [nameof(GenericPageModel<object>.Total).ToLower()] = new JSchema { Type = JSchemaType.Number, Minimum = 0 },
                    [nameof(GenericPageModel<object>.Links).ToLower()] = new JSchema
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
                    nameof(GenericPageModel<object>.Items).ToLower(),
                    nameof(GenericPageModel<object>.Links).ToLower(),
                    nameof(GenericPageModel<object>.Total).ToLower()
                }
        };

        public PatientsControllerTests(ITestOutputHelper outputHelper, IntegrationFixture<Startup> fixture, IdentityApiFixture identityFixture)
        {
            _outputHelper = outputHelper;
            _server = fixture;
            _identityServer = identityFixture;
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

            _outputHelper.WriteLine($"Token : {bearerToken.Jsonify()}");

            using HttpClient client = _server.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, bearerToken.AccessToken.Token);

            // Act
            HttpResponseMessage response = await client.GetAsync(_baseUrl)
                .ConfigureAwait(false);

            // Assert
            ((int)response.StatusCode).Should().Be(Status200OK);
            HttpContentHeaders headers = response.Content.Headers;

            string json = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"json : {json}");

            JToken jToken = JToken.Parse(json);
            jToken.IsValid(_pageResponseSchema).Should().BeTrue();
        }

        public static IEnumerable<object[]> ShouldReturnsSucessCodeCases
        {
            get
            {
                const string url = "/v1/patients";
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
                Email = $"batman_{Guid.NewGuid():N}@gotham.fr",
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
        public async Task Get_With_Empty_Id_Returns_Bad_Request(HttpMethod method)
        {
            _outputHelper.WriteLine($"method : <{method}>");

            // Arrange
            string url = $"{_baseUrl}/{Guid.Empty.ToString()}";
            _outputHelper.WriteLine($"Requested url : <{url}>");

            NewAccountInfo newAccountInfo = new NewAccountInfo
            {
                Username = $"robin_{Guid.NewGuid()}",
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
                           .NotBeNullOrWhiteSpace();
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
            string url = $"{_baseUrl}/search?page=2&page10&firstname=Bruce";
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
            using HttpClient client = _server.CreateClient();
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

            LoginInfo loginInfo = new LoginInfo
            {
                Username = newAccountInfo.Username,
                Password = newAccountInfo.Password
            };

            BearerTokenInfo bearerToken = await _identityServer.RegisterAndLogIn(newAccountInfo)
                .ConfigureAwait(false);

            NewPatientModel newPatient = new NewPatientModel
            {
                Name = "Victor Freeze"
            };

            using HttpClient client = _server.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, bearerToken.AccessToken.Token);

            // Act
            HttpResponseMessage response = await client.PostAsync(_baseUrl, new StringContent(newPatient.Jsonify(), Encoding.UTF8, MediaTypeNames.Application.Json))
                                                       .ConfigureAwait(false);

            string json = await response.Content.ReadAsStringAsync()
                                                .ConfigureAwait(false);
            _outputHelper.WriteLine($"Response : {json}");
            _outputHelper.WriteLine($"HTTP create patient status code : {response.StatusCode}");
            response.IsSuccessStatusCode.Should()
                                        .BeTrue("Creating the resource should succeed");

            Guid patientId = JToken.Parse(json).ToObject<Browsable<PatientInfo>>().Resource.Id;

            NewBloodPressureModel resourceToCreate = new NewBloodPressureModel
            {
                SystolicPressure = 120,
                DiastolicPressure = 80,
                DateOfMeasure = 23.January(2002).AddHours(23).AddMinutes(36)
            };

            JSchema createdResourceSchema = new JSchema
            {
                Type = JSchemaType.Object,
                Properties =
                {
                    [nameof(Browsable<BloodPressureInfo>.Resource).ToLower()] = new JSchema
                    {
                        Type = JSchemaType.Object,
                        Properties =
                        {
                            [nameof(BloodPressureInfo.SystolicPressure).ToLower()] = new JSchema { Type = JSchemaType.Number },
                            [nameof(BloodPressureInfo.DiastolicPressure).ToLower()] = new JSchema { Type = JSchemaType.Number },
                            [nameof(BloodPressureInfo.DateOfMeasure).ToLower()] = new JSchema { Type = JSchemaType.String },
                            [nameof(BloodPressureInfo.UpdatedDate).ToLower()] = new JSchema { Type = JSchemaType.String },
                            [nameof(BloodPressureInfo.Id).ToLower()] = new JSchema { Type = JSchemaType.String },
                        },
                        AllowAdditionalItems = false
                    }
                },
                AllowAdditionalItems = false
            };

            // Act
            string requestUri = $"{_baseUrl}/{patientId}/bloodpressures";
            _outputHelper.WriteLine($"Endpoint : {requestUri}");
            response = await client.PostAsync(requestUri, new StringContent(resourceToCreate.Jsonify(), Encoding.UTF8, MediaTypeNames.Application.Json))
                                   .ConfigureAwait(false);
            _outputHelper.WriteLine($"HTTP create bloodpressure for patient <{patientId}> status code : {response.StatusCode}");
            _outputHelper.WriteLine($"Response content: {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");

            // Assert

            response.IsSuccessStatusCode.Should().BeTrue($"Creating a valid {nameof(BloodPressureInfo)} resource must succeed");
            ((int)response.StatusCode).Should().Be(Status201Created, $"the resource was created");

            json = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            JToken jToken = JToken.Parse(json);
            jToken.IsValid(createdResourceSchema).Should()
                .BeTrue();

            Uri location = response.Headers.Location;
            _outputHelper.WriteLine($"Location of the resource : <{location}>");
            location.Should().NotBeNull();
            location.IsAbsoluteUri.Should().BeTrue("location of the resource must be an absolute URI");
            using HttpRequestMessage headMessage = new HttpRequestMessage(Head, location);
            using HttpResponseMessage checkResponse = await client.SendAsync(headMessage)
                .ConfigureAwait(false);

            checkResponse.IsSuccessStatusCode.Should().BeTrue($"The content location must point to the created resource");
        }

        public static IEnumerable<object[]> InvalidRequestToCreateABloodPressureResourceCases
        {
            get
            {
                yield return new object[]
                {
                    new CreateBloodPressureInfo(),
                    $"No data set onto the resource"
                };

                yield return new object[]
                {
                    new CreateBloodPressureInfo {
                        SystolicPressure = 120,
                        DiastolicPressure = 80,
                        DateOfMeasure = 20.June(2003)
                    },
                    $"No {nameof(CreateBloodPressureInfo.PatientId)} data set onto the resource"
                };
            }
        }

        [Theory]
        [MemberData(nameof(InvalidRequestToCreateABloodPressureResourceCases))]
        public async Task PostInvalidBloodPressure_Returns_BadRequest(CreateBloodPressureInfo invalidResource, string reason)
        {
            // Arrange
            NewPatientInfo newPatientInfo = new NewPatientInfo
            {
                Name = "Solomon Grundy"
            };
            string username = $"batman_{Guid.NewGuid()}";
            NewAccountInfo newAccountInfo = new NewAccountInfo
            {
                Username = username,
                Email = $"{username}@gotham.fr",
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

            _outputHelper.WriteLine($"Token : {bearerToken.Jsonify()}");

            using HttpClient client = _server.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, bearerToken.AccessToken.Token);

            HttpResponseMessage response = await client.PostAsync(_baseUrl, new StringContent(newPatientInfo.Jsonify(), Encoding.UTF8, MediaTypeNames.Application.Json))
                                                             .ConfigureAwait(false);

            string json = await response.Content.ReadAsStringAsync()
                                                .ConfigureAwait(false);
            PatientInfo patientInfo = JsonSerializer.Deserialize<PatientInfo>(json);

            // Act
            _outputHelper.WriteLine($"Invalid resource : {invalidResource}");
            response = await client.PostAsync($"{_baseUrl}/{patientInfo.Id}/bloodpressures", new StringContent(invalidResource.Jsonify(), Encoding.UTF8, MediaTypeNames.Application.Json))
                                   .ConfigureAwait(false);

            // Assert

            _outputHelper.WriteLine($"Response HTTP code : {response.StatusCode}");

            response.IsSuccessStatusCode.Should()
                .BeFalse(reason);
            ((int)response.StatusCode).Should()
                .Be(Status400BadRequest, reason);
            response.ReasonPhrase.Should()
                .NotBeNullOrWhiteSpace();

            string content = await response.Content.ReadAsStringAsync()
               .ConfigureAwait(false);
            _outputHelper.WriteLine($"Response content : {content}");

            JToken.Parse(content).IsValid(_errorObjectSchema)
                    .Should().BeTrue("Validation errors");
        }

        [Fact]
        public async Task GivenPatientExists_AllLinksWithGetMethod_ShouldBe_Valid()
        {
            // Arrange
            NewPatientInfo newPatient = new NewPatientInfo
            {
                Name = "Victor Freeze"
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

            using HttpClient client = _server.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, bearerToken.AccessToken.Token);

            HttpResponseMessage response = await client.PostAsync(_baseUrl, new StringContent(newPatient.Jsonify(), Encoding.UTF8, MediaTypeNames.Application.Json))
                                                       .ConfigureAwait(false);

            _outputHelper.WriteLine($"HTTP create patient status code : {response.StatusCode}");

            string json = await response.Content.ReadAsStringAsync()
                                                .ConfigureAwait(false);

            _outputHelper.WriteLine($"json : {json}");
            IEnumerable<Link> patientLinks = JToken.Parse(json)[nameof(Browsable<PatientInfo>.Links).ToLower()].ToObject<IEnumerable<Link>>();
            IEnumerable<Link> linksToGetData = patientLinks.Where(x => x.Method == "GET" || x.Method == "HEAD");

            using HttpClient client2 = _server.CreateClient();
            _outputHelper.WriteLine($"Checking links accessibility");
            foreach (Link link in linksToGetData)
            {
                _outputHelper.WriteLine($"Link under test : {link}");
                HttpRequestMessage headRequestMessage = new HttpRequestMessage
                {
                    Method = Head,
                    RequestUri = new Uri(link.Href, UriKind.RelativeOrAbsolute)
                };
                headRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, bearerToken.AccessToken.Token);

                // Act
                response = await client2.SendAsync(headRequestMessage)
                                        .ConfigureAwait(false);

                // Assert
                _outputHelper.WriteLine($"HTTP HEAD <{link.Href}> status code : <{response.StatusCode}>");
                _outputHelper.WriteLine($"HEAD Response content : {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");

                response.IsSuccessStatusCode.Should()
                    .BeTrue($"<{link.Href}> should be accessible as it was returned as part of the response after creating patient resource");
            }
        }
    }
}
