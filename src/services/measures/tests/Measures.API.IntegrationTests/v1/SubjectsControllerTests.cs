namespace Measures.API.IntegrationTests.v1
{
    using Bogus;

    using FluentAssertions;
    using FluentAssertions.Extensions;

    using Measures.API.Features.Subjects;
    using Measures.DTO;
    using Measures.Ids;

    using MedEasy.IntegrationTests.Core;
    using MedEasy.RestObjects;

    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Mvc;

    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Schema;

    using NodaTime;
    using NodaTime.Extensions;
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
    [Feature("Patients")]
    public class SubjectsControllerTests : IAssemblyFixture<IntegrationFixture<Startup>>
    {
        private readonly IntegrationFixture<Startup> _server;
        private readonly ITestOutputHelper _outputHelper;

        private readonly Faker _faker;
        private const string _baseUrl = "/subjects";
        private readonly JsonSerializerOptions _serializerOptions;

        private static readonly JSchema _errorObjectSchema = new()
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

        public SubjectsControllerTests(ITestOutputHelper outputHelper, IntegrationFixture<Startup> fixture)
        {
            _faker = new();
            _outputHelper = outputHelper;
            _server = fixture;
            _serializerOptions = new (JsonSerializerDefaults.Web);
        }

        [Fact]
        public async Task GetAll_With_No_Data()
        {
            // Arrange
            using HttpClient client = _server.CreateClient();
            client.DefaultRequestHeaders.Add("api-version", "1.0");

            // Act
            HttpResponseMessage response = await client.GetAsync(_baseUrl)
                .ConfigureAwait(false);

            // Assert
            ((int)response.StatusCode).Should().Be(Status200OK);

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
                const string url = "/subjects";
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
            client.DefaultRequestHeaders.Add("api-version", "1.0");

            HttpRequestMessage message = new(method, url);

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
        public async Task Given_empty_id_Get_returns_NotFound(HttpMethod method)
        {
            _outputHelper.WriteLine($"method : <{method}>");

            // Arrange
            string url = $"{_baseUrl}/{Guid.Empty}";
            _outputHelper.WriteLine($"Requested url : <{url}>");

            using HttpClient client = _server.CreateClient();
            client.DefaultRequestHeaders.Add("api-version", "1.0");

            using HttpRequestMessage message = new(method, url);

            // Act
            HttpResponseMessage response = await client.SendAsync(message)
                .ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should()
                .BeFalse("the requested subject id is empty");
            ((int)response.StatusCode).Should()
                .Be(Status404NotFound, "the requested subject id is empty");
        }

        [Fact]
        public async Task GivenEmptyEndpoint_GetPageTwoOfEmptyResult_Returns_NotFound()
        {
            // Arrange
            string url = $"{_baseUrl}/search?page=2&page10&name=*Bruce*&sort=name";
            _outputHelper.WriteLine($"Requested url : <{url}>");

            using HttpClient client = _server.CreateClient();
            client.DefaultRequestHeaders.Add("api-version", "1.0");

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
            NewSubjectModel newPatient = new()
            {
                Name = "Victor Freeze"
            };

            JsonSerializerOptions jsonSerializerOptions = new();
            jsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            using HttpClient client = _server.CreateClient();
            client.DefaultRequestHeaders.Add("api-version", "1.0");

            // Act
            HttpResponseMessage response = await client.PostAsJsonAsync(_baseUrl, newPatient, jsonSerializerOptions)
                                                       .ConfigureAwait(false);

            _outputHelper.WriteLine($"HTTP create subject status code : {response.StatusCode}");
            response.IsSuccessStatusCode.Should()
                                        .BeTrue("Creating the resource should succeed");

            Browsable<SubjectInfo> browsableSubject = await response.Content.ReadFromJsonAsync<Browsable<SubjectInfo>>()
                                                                     .ConfigureAwait(false);

            SubjectId subjectId = browsableSubject.Resource.Id;
            NewBloodPressureModel resourceToCreate = new()
            {
                SystolicPressure = 120,
                DiastolicPressure = 80,
                DateOfMeasure = 23.January(2002).Add(23.Hours().And(36.Minutes())).AsUtc().ToInstant()
            };

            // Act
            string requestUri = $"{_baseUrl}/{subjectId}/bloodpressures";
            _outputHelper.WriteLine($"Endpoint : {requestUri}");
            response = await client.PostAsJsonAsync(requestUri, resourceToCreate, jsonSerializerOptions)
                                   .ConfigureAwait(false);
            _outputHelper.WriteLine($"HTTP create bloodpressure for subject <{subjectId}> status code : {response.StatusCode}");
            _outputHelper.WriteLine($"Response content: {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue($"Creating a valid {nameof(BloodPressureInfo)} resource must succeed");
            ((int)response.StatusCode).Should().Be(Status201Created, $"the resource was created");

            Uri location = response.Headers.Location;
            _outputHelper.WriteLine($"Location of the resource : <{location}>");
            location.Should().NotBeNull();
            location.IsAbsoluteUri.Should().BeTrue("location of the resource must be an absolute URI");
            using HttpRequestMessage headMessage = new(Head, location);
            using HttpResponseMessage checkResponse = await client.SendAsync(headMessage)
                .ConfigureAwait(false);

            checkResponse.IsSuccessStatusCode.Should().BeTrue("The content location must point to the created resource");
        }

        public static IEnumerable<object[]> InvalidRequestToCreateABloodPressureResourceCases
        {
            get
            {
                yield return new object[]
                {
                    new NewBloodPressureModel(),
                    "No data set onto the resource"
                };

                yield return new object[]
                {
                    new NewBloodPressureModel {
                        SystolicPressure = 120,
                        DiastolicPressure = 80,
                        DateOfMeasure = 20.June(2003).AsUtc().ToInstant()
                    },
                    $"No {nameof(CreateBloodPressureInfo.SubjectId)} data set onto the resource"
                };
            }
        }

        [Theory]
        [MemberData(nameof(InvalidRequestToCreateABloodPressureResourceCases))]
        public async Task Given_invalid_BloodPressure_Post_should_return_BadRequest(NewBloodPressureModel invalidResource, string reason)
        {
            // Arrange
            NewSubjectInfo newPatientInfo = new()
            {
                Name = _faker.Person.FullName
            };
            
            using HttpClient client = _server.CreateClient();
            client.DefaultRequestHeaders.Add("api-version", "1");

            HttpResponseMessage response = await client.PostAsJsonAsync(_baseUrl, newPatientInfo, _serializerOptions)
                                                       .ConfigureAwait(false);
            string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            _outputHelper.WriteLine($"Content : {content}");
            Browsable<SubjectInfo> browsableSubjectInfo = await response.Content.ReadFromJsonAsync<Browsable<SubjectInfo>>(_serializerOptions)
                                                                                .ConfigureAwait(false);

            // Act
            _outputHelper.WriteLine($"Invalid resource : {invalidResource.Jsonify()}");
            string requestUri = $"{_baseUrl}/{browsableSubjectInfo.Resource.Id}/bloodpressures";
            _outputHelper.WriteLine($"URL : {requestUri}");
            response = await client.PostAsJsonAsync(requestUri, invalidResource, _serializerOptions)
                                   .ConfigureAwait(false);

            // Assert

            _outputHelper.WriteLine($"Response HTTP code : {response.StatusCode}");

            response.IsSuccessStatusCode.Should()
                .BeFalse(reason);
            ((int)response.StatusCode).Should()
                .Be(Status400BadRequest, reason);
            response.ReasonPhrase.Should()
                .NotBeNullOrWhiteSpace();

            content = await response.Content.ReadAsStringAsync()
                                                   .ConfigureAwait(false);
            _outputHelper.WriteLine($"Response content : {content}");

            JToken.Parse(content).IsValid(_errorObjectSchema)
                    .Should().BeTrue("Validation errors");
        }

        [Fact]
        public async Task GivenPatientExists_AllLinksWithGetMethod_ShouldBe_Valid()
        {
            // Arrange
            NewSubjectInfo newPatient = new()
            {
                Name = "Victor Freeze"
            };

            using HttpClient client = _server.CreateClient();
            client.DefaultRequestHeaders.Add("api-version", "1.0");

            HttpResponseMessage response = await client.PostAsJsonAsync(_baseUrl, newPatient)
                                                       .ConfigureAwait(false);

            _outputHelper.WriteLine($"HTTP create subject status code : {response.StatusCode}");

            string json = await response.Content.ReadAsStringAsync()
                                                .ConfigureAwait(false);

            _outputHelper.WriteLine($"json : {json}");
            IEnumerable<Link> subjectLinks = JToken.Parse(json)[nameof(Browsable<SubjectInfo>.Links).ToLower()].ToObject<IEnumerable<Link>>();
            IEnumerable<Link> linksToGetData = subjectLinks.Where(x => x.Method == "GET" || x.Method == "HEAD");

            using HttpClient client2 = _server.CreateClient();
            client.DefaultRequestHeaders.Add("api-version", "1.0");

            _outputHelper.WriteLine("Checking links accessibility");
            foreach (Link link in linksToGetData)
            {
                _outputHelper.WriteLine($"Link under test : {link}");
                HttpRequestMessage headRequestMessage = new()
                {
                    Method = Head,
                    RequestUri = new Uri(link.Href, UriKind.RelativeOrAbsolute)
                };
                headRequestMessage.Headers.Add("api-version", "1.0");

                // Act
                response = await client2.SendAsync(headRequestMessage)
                                        .ConfigureAwait(false);

                // Assert
                _outputHelper.WriteLine($"HTTP HEAD <{link.Href}> status code : <{response.StatusCode}>");
                _outputHelper.WriteLine($"HEAD Response content : {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");

                response.IsSuccessStatusCode.Should()
                    .BeTrue($"<{link.Href}> should be accessible as it was returned as part of the response after creating subject resource");
            }
        }
    }
}
