namespace Agenda.API.IntegrationTests.v1
{
    using Agenda.API.Resources.v1;
    using Agenda.API.Resources.v1.Appointments;
    using Agenda.Ids;
    using Agenda.Models.v1.Appointments;
    using Agenda.Models.v1.Attendees;

    using FluentAssertions;

    using MedEasy.Core.Filters;
    using MedEasy.IntegrationTests.Core;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Schema;

    using NodaTime;
    using NodaTime.Serialization.SystemTextJson;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Security.Claims;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;
    using Xunit.Extensions.AssemblyFixture;

    using static Microsoft.AspNetCore.Http.StatusCodes;
    using static Newtonsoft.Json.JsonConvert;

    [IntegrationTest]
    [Feature("Agenda")]
    [Feature("Attendees")]
    public class AttendeesControllerTests : IAssemblyFixture<IntegrationFixture<Startup>>
    {
        private readonly IntegrationFixture<Startup> _server;
        private readonly ITestOutputHelper _outputHelper;
        private readonly string _endpointUrl = AttendeesController.EndpointName;

        private static readonly JSchema ErrorObjectSchema = new()
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

        public AttendeesControllerTests(ITestOutputHelper outputHelper, IntegrationFixture<Startup> fixture)
        {
            _outputHelper = outputHelper;
            _server = fixture;
        }

        public static IEnumerable<object[]> GetAll_With_Invalid_Pagination_Returns_BadRequestCases
        {
            get
            {
                int[] invalidPages = { int.MinValue, -1, -10, 0, 1, 5, 10 };

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
            string url = $"{_endpointUrl}?page={page}&pageSize={pageSize}";

            _outputHelper.WriteLine($"Url under test : <{url}>");

            using HttpClient client = _server.CreateClient();
            client.DefaultRequestHeaders.Add("api-version", "1");

            // Act
            using HttpResponseMessage response = await client.GetAsync(url)
                                                             .ConfigureAwait(false);

            // Assert
            string content = await response.Content.ReadAsStringAsync()
                                                   .ConfigureAwait(false);

            _outputHelper.WriteLine($"Response content : {content}");

            response.IsSuccessStatusCode.Should().BeFalse("Invalid page and/or pageSize");
            ((int)response.StatusCode).Should().Be(Status400BadRequest);

            content.Should()
                   .NotBeNullOrEmpty("BAD REQUEST content must provide additional information on errors");

            JToken token = JToken.Parse(content);
            token.IsValid(ErrorObjectSchema)
                .Should().BeTrue("Error object must be provided when API returns BAD REQUEST");

            ValidationProblemDetails errorObject = token.ToObject<ValidationProblemDetails>();
            errorObject.Title.Should()
                .Be("One or more validation errors occurred.");
            errorObject.Errors.Should()
                .NotBeEmpty();

            errorObject.Errors.ContainsKey("page").Should()
                              .Be(page <= 0, "page <= 0 is not a valid value");

            errorObject.Errors.ContainsKey("pageSize").Should()
                              .Be(pageSize <= 0, "pageSize <= 0 is not a valid value");
        }

        public static IEnumerable<object[]> InvalidSearchCases
        {
            get
            {
                yield return new object[]
                {
                    "?page=-1" ,
                    (Expression<Func<ValidationProblemDetails, bool>>)(err => err.Status == Status400BadRequest
                        && !string.IsNullOrWhiteSpace(err.Title)
                        && err.Errors != null
                        && err.Errors.Once(kv => kv.Key.Equals("page", StringComparison.OrdinalIgnoreCase))
                    ),
                    $"{nameof(SearchAppointmentModel.Page)} must be greater than 1"
                };

                yield return new object[]
                {
                    "?pageSize=-1" ,
                    (Expression<Func<ValidationProblemDetails, bool>>)(err => err.Status == Status400BadRequest
                        && !string.IsNullOrWhiteSpace(err.Title)
                        && err.Errors != null
                        && err.Errors.Once(kv => kv.Key.Equals("pageSize", StringComparison.OrdinalIgnoreCase))
                    ),
                    $"{nameof(SearchAppointmentModel.PageSize)} must be greater than 1"
                };
            }
        }

        [Theory]
        [MemberData(nameof(InvalidSearchCases))]
        public async Task GivenInvalidCriteria_Search_Returns_BadRequest(string queryString, Expression<Func<ValidationProblemDetails, bool>> errorObjectExpectation, string reason)
        {
            _outputHelper.WriteLine($"search query string : {queryString}");

            IEnumerable<Claim> claims = new[]
            {
                new Claim(ClaimTypes.Name, "Bruce Wayne")
            };

            // Arrange
            string url = $"{_endpointUrl}/{nameof(AttendeesController.Search)}{queryString}";
            _outputHelper.WriteLine($"Url under test : <{url}>");

            // Act
            using HttpClient client = _server.CreateClient();
            client.DefaultRequestHeaders.Add("api-version", "1");
            HttpResponseMessage response = await client.GetAsync(url)
                                                       .ConfigureAwait(false);

            // Assert
            string content = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"Response content : {content}");

            response.IsSuccessStatusCode.Should().BeFalse("Invalid search criteria");
            ((int)response.StatusCode).Should().Be(Status400BadRequest);

            content.Should()
                .NotBeNullOrEmpty("BAD REQUEST content must provide additional information on errors");

            JToken token = JToken.Parse(content);
            token.IsValid(ErrorObjectSchema)
                .Should().BeTrue($"Error object must be provided when HTTP GET <{url}> returns BAD REQUEST");

            ValidationProblemDetails errorObject = token.ToObject<ValidationProblemDetails>();
            errorObject.Should().Match(errorObjectExpectation, reason);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("HEAD")]
        public async Task Search_Handles_Verb(string verb)
        {
            IEnumerable<Claim> claims = new[]
            {
                new Claim(ClaimTypes.Name, "Bruce Wayne")
            };

            // Arrange
            string url = $"{_endpointUrl}/search?sort=+name&page=1";

            HttpRequestMessage request = new(new HttpMethod(verb), url);
            request.Headers.Add("version", "1");
            using HttpClient client = _server.CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request)
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"Response content :{await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");

            // Assert
            response.IsSuccessStatusCode.Should()
                                        .BeTrue($"HTTP {response.Version} {verb} {url} must be supported");
        }

        public static IEnumerable<object[]> GetCountCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<NewAppointmentModel>(),
                    "?page=1&pageSize=10",
                    (total : 0, count : 0)
                };

                {
                    IEnumerable<AttendeeModel> participants = new[]
                    {
                        new AttendeeModel {
                            Id = AttendeeId.New(),
                            Name = "Ed Nygma {Guid.NewGuid()}"
                        },
                        new AttendeeModel {
                            Id = AttendeeId.New(),
                            Name = "Oswald Coblepot"}
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetCountCases))]
        public async Task Enpoint_Provides_CountsHeaders(IEnumerable<NewAppointmentModel> newAppointments, string url, (int total, int count) expectedCountHeaders)
        {
            // Arrange
            _outputHelper.WriteLine($"Nb items to create : {newAppointments.Count()}");
            IEnumerable<Claim> claims = new[]
            {
                new Claim(ClaimTypes.Name, "Bruce Wayne")
            };
            using HttpClient client = _server.CreateClient();
            client.DefaultRequestHeaders.Add("api-version", "1.0");

            string requestUri = AppointmentsController.EndpointName;
            await newAppointments.ForEachAsync(async (newParticipant) =>
                                               {
                                                   JsonSerializerOptions jsonSerializerOptions = new();
                                                   jsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                                                   using HttpResponseMessage createdResponse = await client.PostAsJsonAsync(requestUri, newParticipant, jsonSerializerOptions)
                                                                                                           .ConfigureAwait(false);

                                                   _outputHelper.WriteLine($"{nameof(createdResponse)} status : {createdResponse.StatusCode}");
                                                   _outputHelper.WriteLine($"Created participant response : {await createdResponse.Content.ReadAsStringAsync().ConfigureAwait(false)}");
                                               })
                                               .ConfigureAwait(false);

            string path = $"{_endpointUrl}{url}";
            _outputHelper.WriteLine($"path under test : {path}");

            using HttpRequestMessage request = new(HttpMethod.Head, path);

            // Act
            using HttpResponseMessage response = await client.SendAsync(request)
                                                             .ConfigureAwait(false);

            // Assert
            _outputHelper.WriteLine($"Response content : {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");
            _outputHelper.WriteLine($"Response status code : {response.StatusCode}");
            response.IsSuccessStatusCode.Should().BeTrue();

            _outputHelper.WriteLine($"Response headers :{response.Headers.Jsonify()}");

            response.Headers.Should()
                .ContainSingle(header => header.Key == AddCountHeadersFilterAttribute.TotalCountHeaderName).And
                .ContainSingle(header => header.Key == AddCountHeadersFilterAttribute.CountHeaderName);

            IEnumerable<string> totalCountHeaderValues = response.Headers.GetValues(AddCountHeadersFilterAttribute.TotalCountHeaderName);
            totalCountHeaderValues.Should()
                                  .HaveCount(1);

            IEnumerable<string> countHeaderValues = response.Headers.GetValues(AddCountHeadersFilterAttribute.CountHeaderName);
            countHeaderValues.Should()
                             .HaveCount(1);
        }
    }
}
