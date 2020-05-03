using Agenda.API.Resources.v1;
using Agenda.API.Resources.v1.Appointments;
using Agenda.Models.v1.Appointments;
using Agenda.Models.v1.Attendees;
using FluentAssertions;
using FluentAssertions.Extensions;
using MedEasy.Core.Filters;
using MedEasy.IntegrationTests.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Newtonsoft.Json.JsonConvert;

namespace Agenda.API.IntegrationTests.v1
{
    [IntegrationTest]
    [Feature("Agenda")]
    [Feature("Attendees")]
    public class AttendeesControllerTests : IClassFixture<IntegrationFixture<Startup>>, IDisposable
    {
        private readonly IntegrationFixture<Startup> _server;
        private ITestOutputHelper _outputHelper;
        private const string _rootEndpointUrl = "/v1";
        private const string _endpointUrl = _rootEndpointUrl + "/attendees";

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

        /// <summary>
        /// Schema of an <see cref="AttendeeModel"/> resource once translated to json
        /// </summary>
        private static readonly JSchema _participantInfoResourceSchema = new JSchema
        {
            Type = JSchemaType.Object,
            Properties =
            {
                [nameof(AttendeeModel.Id).ToCamelCase()] = new JSchema { Type = JSchemaType.String },
                [nameof(AttendeeModel.Name).ToCamelCase()] = new JSchema { Type = JSchemaType.String },
                [nameof(AttendeeModel.Email).ToCamelCase()] = new JSchema { Type = JSchemaType.String },
                [nameof(AttendeeModel.PhoneNumber).ToCamelCase()] = new JSchema { Type = JSchemaType.String  },
                [nameof(AttendeeModel.UpdatedDate).ToCamelCase()] = new JSchema { Type = JSchemaType.String  },
            },
            Required =
            {
                nameof(AttendeeModel.Id).ToCamelCase(),
                nameof(AttendeeModel.Name).ToCamelCase(),
                nameof(AttendeeModel.Email).ToCamelCase(),
                nameof(AttendeeModel.PhoneNumber).ToCamelCase()
            }
        };

        public AttendeesControllerTests(ITestOutputHelper outputHelper, IntegrationFixture<Startup> fixture)
        {
            _outputHelper = outputHelper;
            _server = fixture;
        }

        public void Dispose()
        {
            _outputHelper = null;
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

            // Act
            using HttpClient client = _server.CreateClient();

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
            token.IsValid(_errorObjectSchema)
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

            // Arrange
            string url = $"{_endpointUrl}/{nameof(AttendeesController.Search)}{queryString}";
            _outputHelper.WriteLine($"Url under test : <{url}>");

            // Act
            using HttpClient client = _server.CreateClient();
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
            token.IsValid(_errorObjectSchema)
                .Should().BeTrue($"Error object must be provided when HTTP GET <{url}> returns BAD REQUEST");

            ValidationProblemDetails errorObject = token.ToObject<ValidationProblemDetails>();
            errorObject.Should().Match(errorObjectExpectation, reason);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("HEAD")]
        public async Task Search_Handles_Verb(string verb)
        {
            // Arrange
            string url = $"{_endpointUrl}/search?sort=+name&page=1";

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(verb), url);
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
                        new AttendeeModel {Name = "Ed Nygma"},
                        new AttendeeModel {Name = "Oswald Coblepot"}
                    };

                    yield return new object[]
                    {
                        new []{
                            new NewAppointmentModel { Location = "The bowlery", Attendees = participants, StartDate = 1.April(2012), EndDate = 2.April(2012), Subject = "Let's rob something !" }
                        },
                        $"/search?{new SearchAttendeeModel { Name="*Nygma*|*Coblepot*", Page=1, PageSize=30 }.ToQueryString()}",
                        (total : 2, count : 2)
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
            using HttpClient client = _server.CreateClient();

            await newAppointments.ForEachAsync(async (newParticipant) =>
            {
                HttpResponseMessage createdResponse = await client.PostAsync($"agenda/{AppointmentsController.EndpointName}", new StringContent(SerializeObject(newParticipant), Encoding.UTF8, "application/json"))
                    .ConfigureAwait(false);

                _outputHelper.WriteLine($"{nameof(createdResponse)} status : {createdResponse.StatusCode}");
            })
            .ConfigureAwait(false);

            string path = $"{_endpointUrl}{url}";
            _outputHelper.WriteLine($"path under test : {path}");

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, path);

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

            response.Headers.GetValues(AddCountHeadersFilterAttribute.TotalCountHeaderName).Should()
                .HaveCount(1);

            response.Headers.GetValues(AddCountHeadersFilterAttribute.CountHeaderName).Should()
                .HaveCount(1);
            
        }
    }
}
