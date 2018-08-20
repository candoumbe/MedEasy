using FluentAssertions;
using FluentAssertions.Extensions;
using Identity.API.Fixtures;
using Identity.DataStores.SqlServer;
using Identity.DTO;
using Measures.API.Features.Patients;
using Measures.Context;
using Measures.DTO;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Microsoft.AspNetCore.Http.HttpMethods;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Newtonsoft.Json.JsonConvert;

namespace Measures.API.IntegrationTests
{
    [IntegrationTest]
    [Feature("Patients")]
    public class PatientsControllerTests : IDisposable, IClassFixture<ServicesTestFixture<Startup>>, IClassFixture<ServicesTestFixture<Identity.API.Startup>>
    {
        private TestServer _server;
        private ITestOutputHelper _outputHelper;
        private TestServer _identityServer;
        private const string _endpointUrl = "/measures/patients";
        private static readonly JSchema _errorObjectSchema = new JSchema
        {
            Type = JSchemaType.Object,
            Properties =
            {
                [nameof(ErrorObject.Code).ToLower()] = new JSchema { Type = JSchemaType.String},
                [nameof(ErrorObject.Description).ToLower()] = new JSchema { Type = JSchemaType.String},
                [nameof(ErrorObject.Errors).ToLower()] = new JSchema { Type = JSchemaType.Object },
            },
            Required =
            {
                nameof(ErrorObject.Code).ToLower(),
                nameof(ErrorObject.Description).ToLower(),
                nameof(ErrorObject.Errors).ToLower()
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
                    [nameof(GenericPagedGetResponse<object>.Count).ToLower()] = new JSchema { Type = JSchemaType.Number, Minimum = 0 },
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
                    nameof(GenericPagedGetResponse<object>.Count).ToLower()
                }

        };

        public PatientsControllerTests(ITestOutputHelper outputHelper, ServicesTestFixture<Startup> fixture, ServicesTestFixture<Identity.API.Startup> identityFixture)
        {
            _outputHelper = outputHelper;
            fixture.Initialize(
                relativeTargetProjectParentDir: Path.Combine("..", "..", "..", "..", "src", "services", "Measures"),
                environmentName: "IntegrationTest",
                applicationName: typeof(Startup).Assembly.GetName().Name);
            _server = fixture.Server;

            identityFixture.Initialize<Identity.API.Startup>(
                relativeTargetProjectParentDir: Path.Combine("..", "..", "..", "..", "src", "services", "Identity"),
                environmentName: "IntegrationTest",
                applicationName: typeof(Identity.API.Startup).Assembly.GetName().Name);

            _identityServer = identityFixture.Server;
        }


        public void Dispose()
        {
            _outputHelper = null;
            _server?.Dispose();
            _server = null;

            _identityServer?.Dispose();
            _identityServer = null;
        }

        [Fact]
        public async Task GetAll_With_No_Data()
        {
            // Arrange
            NewAccountInfo newAccountInfo = new NewAccountInfo
            {
                Username = "batman",
                Email = "batman@gotham.fr",
                Password = "thecapedcrusader",
                ConfirmPassword = "thecapedcrusader"
            };

            LoginInfo loginInfo = new LoginInfo
            {
                Username = newAccountInfo.Username,
                Password = newAccountInfo.Password
            };

            BearerTokenInfo bearerToken = await IdentityApiFixture.Register(_identityServer, newAccountInfo)
                .ConfigureAwait(false);

            RequestBuilder rb = _server.CreateRequest("/measures/patients")
                .AddHeader("Accept", "application/json")
                .AddHeader("Accept-Charset", "utf-8")
                .AddHeader("Authorization", $"{JwtBearerDefaults.AuthenticationScheme} {bearerToken.AccessToken}");

            // Act
            HttpResponseMessage response = await rb.GetAsync()
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



        [Theory]
        [InlineData("/measures/patients", "GET")]
        [InlineData("/measures/patients", "HEAD")]
        [InlineData("/measures/patients", "OPTIONS")]
        public async Task ShouldReturnsSuccessCode(string url, string method)
        {

            // Arrange
            NewAccountInfo newAccountInfo = new NewAccountInfo
            {
                Username = "batman",
                Email = "batman@gotham.fr",
                Password = "thecapedcrusader",
                ConfirmPassword = "thecapedcrusader"
            };

            LoginInfo loginInfo = new LoginInfo
            {
                Username = newAccountInfo.Username,
                Password = newAccountInfo.Password
            };

            BearerTokenInfo bearerToken = await IdentityApiFixture.Register(_identityServer, newAccountInfo)
                .ConfigureAwait(false);
            RequestBuilder rb = _server.CreateRequest(url)
                .AddHeader("Authorization", $"{JwtBearerDefaults.AuthenticationScheme} {bearerToken.AccessToken}");

            // Act
            HttpResponseMessage response = await rb.SendAsync(method)
                .ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should()
                .BeTrue($"'{method}' HTTP method must be supported");
            ((int)response.StatusCode).Should().Be(Status200OK);

        }

        [Theory]
        [InlineData("HEAD")]
        [InlineData("GET")]
        [InlineData("DELETE")]
        [InlineData("OPTIONS")]
        public async Task Get_With_Empty_Id_Returns_Bad_Request(string method)
        {
            _outputHelper.WriteLine($"method : <{method}>");

            // Arrange
            string url = $"{_endpointUrl}/{Guid.Empty.ToString()}";
            _outputHelper.WriteLine($"Requested url : <{url}>");

            NewAccountInfo newAccountInfo = new NewAccountInfo
            {
                Username = "batman",
                Email = "batman@gotham.fr",
                Password = "thecapedcrusader",
                ConfirmPassword = "thecapedcrusader"
            };

            LoginInfo loginInfo = new LoginInfo
            {
                Username = newAccountInfo.Username,
                Password = newAccountInfo.Password
            };

            BearerTokenInfo bearerToken = await IdentityApiFixture.Register(_identityServer, newAccountInfo)
                .ConfigureAwait(false);

            RequestBuilder requestBuilder = new RequestBuilder(_server, url)
                .AddHeader("Accept", "application/json")
                .AddHeader("Authorization", $"{JwtBearerDefaults.AuthenticationScheme} {bearerToken.AccessToken}");

            // Act
            HttpResponseMessage response = await requestBuilder.SendAsync(method)
                .ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should()
                .BeFalse("the requested patient id is empty");
            ((int)response.StatusCode).Should()
                .Be(Status400BadRequest, "the requested patient id is empty");

            ((int)response.StatusCode).Should().Be(Status400BadRequest, "the requested patient id must not be empty and it's part of the url");

            if (IsGet(method))
            {
                string content = await response.Content.ReadAsStringAsync()
                        .ConfigureAwait(false);

                _outputHelper.WriteLine($"Bad request content : {content}");

                content.Should()
                    .NotBeNullOrEmpty();

                JToken token = JToken.Parse(content);
                token.IsValid(_errorObjectSchema)
                    .Should().BeTrue("Error object must be provided when API returns BAD REQUEST");

                ErrorObject errorObject = token.ToObject<ErrorObject>();
                errorObject.Code.Should()
                    .Be("BAD_REQUEST");
                errorObject.Description.Should()
                    .Be("Validation failed");
                errorObject.Errors.Should()
                    .HaveCount(1).And
                    .ContainKey("id").WhichValue.Should()
                        .HaveCount(1).And
                        .HaveElementAt(0, $"'id' must have a non default value");
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
                Username = "batman",
                Email = "batman@gotham.fr",
                Password = "thecapedcrusader",
                ConfirmPassword = "thecapedcrusader"
            };

            LoginInfo loginInfo = new LoginInfo
            {
                Username = newAccountInfo.Username,
                Password = newAccountInfo.Password
            };

            BearerTokenInfo bearerToken = await IdentityApiFixture.Register(_identityServer, newAccountInfo)
                .ConfigureAwait(false);
            RequestBuilder requestBuilder = new RequestBuilder(_server, url)
                .AddHeader("Accept", "application/json")
                .AddHeader("Authorization", $"{JwtBearerDefaults.AuthenticationScheme} {bearerToken.AccessToken}");

            // Act
            HttpResponseMessage response = await requestBuilder.GetAsync()
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
                Username = "batman",
                Email = "batman@gotham.fr",
                Password = "thecapedcrusader",
                ConfirmPassword = "thecapedcrusader"
            };

            LoginInfo loginInfo = new LoginInfo
            {
                Username = newAccountInfo.Username,
                Password = newAccountInfo.Password
            };

            BearerTokenInfo bearerToken = await IdentityApiFixture.Register(_identityServer, newAccountInfo)
                .ConfigureAwait(false);

            NewPatientModel newPatient = new NewPatientModel
            {
                Firstname = "Victor",
                Lastname = "Freeze"
            };
            RequestBuilder requestBuilder = _server.CreateRequest("/measures/patients")
                .AddHeader("Accept", "application/json")
                .AddHeader("Authorization", $"{JwtBearerDefaults.AuthenticationScheme} {bearerToken.AccessToken}")
                .And(request => request.Content = new StringContent(SerializeObject(newPatient), Encoding.UTF8, "application/json"));

            HttpResponseMessage response = await requestBuilder.PostAsync()
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"HTTP create patient status code : {response.StatusCode}");

            string json = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"created resource : {json}");

            string patientId = JToken.Parse(json)[nameof(BrowsableResource<PatientInfo>.Resource).ToLower()][nameof(PatientInfo.Id).ToLower()].ToString();

            NewBloodPressureModel resourceToCreate = new NewBloodPressureModel
            {
                SystolicPressure = 120,
                DiastolicPressure = 80,
                DateOfMeasure = 23.January(2002).AddHours(23).AddMinutes(36),

            };

            JSchema createdResourceSchema = new JSchema
            {
                Type = JSchemaType.Object,
                Properties =
                {
                    [nameof(BrowsableResource<BloodPressureInfo>.Resource).ToLower()] = new JSchema
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

            requestBuilder = _server.CreateRequest($"{_endpointUrl}/{patientId}/bloodpressures")
                .AddHeader("Content-Type", "application/json")
                .AddHeader("Authorization", $"{JwtBearerDefaults.AuthenticationScheme} {bearerToken.AccessToken}")
                .And((request) =>
                    request.Content = new StringContent(SerializeObject(resourceToCreate), Encoding.UTF8, "application/json")
                );

            // Act
            response = await requestBuilder.PostAsync()
                .ConfigureAwait(false);
            _outputHelper.WriteLine($"HTTP create bloodpressure for patient <{patientId}> status code : {response.StatusCode}");

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

            requestBuilder = _server.CreateRequest(location.ToString())
                .AddHeader("Authorization", $"{JwtBearerDefaults.AuthenticationScheme} {bearerToken.AccessToken}")
                ;

            HttpResponseMessage checkResponse = await requestBuilder.SendAsync(Head)
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
                Firstname = "Solomon",
                Lastname = "Grundy"
            };
            NewAccountInfo newAccountInfo = new NewAccountInfo
            {
                Username = "batman",
                Email = "batman@gotham.fr",
                Password = "thecapedcrusader",
                ConfirmPassword = "thecapedcrusader"
            };

            LoginInfo loginInfo = new LoginInfo
            {
                Username = newAccountInfo.Username,
                Password = newAccountInfo.Password
            };

            BearerTokenInfo bearerToken = await IdentityApiFixture.Register(_identityServer, newAccountInfo)
                .ConfigureAwait(false);
            RequestBuilder requestBuilder = new RequestBuilder(_server, _endpointUrl)
                .AddHeader("Authorization", $"{JwtBearerDefaults.AuthenticationScheme} {bearerToken.AccessToken}")
                .And(request => request.Content = new StringContent(SerializeObject(newPatientInfo), Encoding.UTF8, "application/json"));

            HttpResponseMessage response = await requestBuilder.PostAsync()
                .ConfigureAwait(false);

            string json = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            PatientInfo patientInfo = DeserializeObject<PatientInfo>(json);

            requestBuilder = new RequestBuilder(_server, $"{_endpointUrl}/{patientInfo.Id}/bloodpressures")
                .AddHeader("Authorization", $"{JwtBearerDefaults.AuthenticationScheme} {bearerToken.AccessToken}")
                .And(request => request.Content = new StringContent(SerializeObject(invalidResource), Encoding.UTF8, "application/json"));

            // Act
            response = await requestBuilder.PostAsync()
                .ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should()
                .BeFalse(reason);
            ((int)response.StatusCode).Should()
                .Be(Status422UnprocessableEntity, reason);
            response.ReasonPhrase.Should()
                .NotBeNullOrWhiteSpace();

            string content = await response.Content.ReadAsStringAsync()
               .ConfigureAwait(false);


            JToken.Parse(content).IsValid(_errorObjectSchema)
                .Should().BeTrue("Validation errors");

        }

        [Fact]
        public async Task GivenPatientExists_AllLinksWithGetMethod_ShouldBe_Valid()
        {
            // Arrange
            NewPatientInfo newPatient = new NewPatientInfo
            {
                Firstname = "Victor",
                Lastname = "Freeze"
            };

            NewAccountInfo newAccountInfo = new NewAccountInfo
            {
                Username = "batman",
                Email = "batman@gotham.fr",
                Password = "thecapedcrusader",
                ConfirmPassword = "thecapedcrusader"
            };

            LoginInfo loginInfo = new LoginInfo
            {
                Username = newAccountInfo.Username,
                Password = newAccountInfo.Password
            };

            BearerTokenInfo bearerToken = await IdentityApiFixture.Register(_identityServer, newAccountInfo)
                .ConfigureAwait(false);
            RequestBuilder requestBuilder = new RequestBuilder(_server, "/measures/patients")
                .AddHeader("Accept", "application/json")
                .AddHeader("Authorization", $"{JwtBearerDefaults.AuthenticationScheme} {bearerToken.AccessToken}")
                .And(request => request.Content = new StringContent(SerializeObject(newPatient), Encoding.UTF8, "application/json"));

            HttpResponseMessage response = await requestBuilder.PostAsync()
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"HTTP create patient status code : {response.StatusCode}");

            string json = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            _outputHelper.WriteLine($"json : {json}");
            IEnumerable<Link> patientLinks = JToken.Parse(json)[nameof(BrowsableResource<PatientInfo>.Links).ToLower()].ToObject<IEnumerable<Link>>();
            IEnumerable<Link> linksToGetData = patientLinks.Where(x => IsGet(x.Method));

            foreach (Link link in linksToGetData)
            {
                requestBuilder = new RequestBuilder(_server, link.Href)
                    .AddHeader("Authorization", $"{JwtBearerDefaults.AuthenticationScheme} {bearerToken.AccessToken}")
                    .AddHeader("Accept", "application/json");

                // Act
                response = await requestBuilder.SendAsync(Head)
                    .ConfigureAwait(false);


                // Assert
                _outputHelper.WriteLine($"HTTP HEAD <{link.Href}> status code : <{response.StatusCode}>");
                response.IsSuccessStatusCode.Should()
                    .BeTrue($"<{link.Href}> should be accessible as it was returned as part of the response after creating patient resource");
            }

        }
    }
}
