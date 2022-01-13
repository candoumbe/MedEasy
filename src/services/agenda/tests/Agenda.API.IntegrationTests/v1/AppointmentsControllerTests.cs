namespace Agenda.API.IntegrationTests.v1
{
    using Agenda.API.Resources.v1;
    using Agenda.API.Resources.v1.Appointments;
    using Agenda.Ids;
    using Agenda.Models.v1.Appointments;
    using Agenda.Models.v1.Attendees;

    using Bogus;

    using FluentAssertions;
    using FluentAssertions.Extensions;

    using MedEasy.Core.Filters;
    using MedEasy.IntegrationTests.Core;
    using MedEasy.RestObjects;

    using Microsoft.AspNetCore.Mvc;

    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Schema;

    using NodaTime;
    using NodaTime.Extensions;
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

    using static MedEasy.RestObjects.LinkRelation;
    using static Microsoft.AspNetCore.Http.StatusCodes;

    [IntegrationTest]
    [Feature("Agenda")]
    [Feature("Appointments")]
    public class AppointmentsControllerTests : IAssemblyFixture<IntegrationFixture<Startup>>
    {
        private readonly IntegrationFixture<Startup> _sut;
        private readonly ITestOutputHelper _outputHelper;
        private const string _endpointUrl = "/appointments";

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
                nameof(ValidationProblemDetails.Status).ToLower()
            }
        };

        private static JsonSerializerOptions SerializerOptions
        {
            get
            {
                JsonSerializerOptions options = new JsonSerializerOptions().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                options.PropertyNameCaseInsensitive = true;

                return options;
            }
        }

        public AppointmentsControllerTests(ITestOutputHelper outputHelper, IntegrationFixture<Startup> fixture)
        {
            _outputHelper = outputHelper;
            _sut = fixture;
        }

        public static IEnumerable<object[]> GetAll_With_Invalid_Pagination_Returns_BadRequestCases
        {
            get
            {
                int[] invalidPages = { int.MinValue, -1, -10, 0, 1, 5, 10 };

                IEnumerable<(int page, int pageSize)> invalidCases = invalidPages.CrossJoin(invalidPages)
                                                                                 .Where(((int page, int pageSize) input) => input.page <= 0 || input.pageSize <= 0);

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
            _outputHelper.WriteLine($"Paging configuration : {new { page, pageSize }.Jsonify()}");

            // Arrange
            string url = $"{_endpointUrl}?page={page}&pageSize={pageSize}";
            _outputHelper.WriteLine($"Url under test : <{url}>");
            IEnumerable<Claim> claims = new[]
            {
                new Claim(ClaimTypes.Name, "Bruce Wayne")
            };
            using HttpClient client = _sut.CreateClient();
            client.DefaultRequestHeaders.Add("api-version", "1.0");

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
            token.IsValid(ErrorObjectSchema).Should()
                                             .BeTrue("Error object must be provided when API returns BAD REQUEST");

            ValidationProblemDetails errorObject = token.ToObject<ValidationProblemDetails>();
            errorObject.Title.Should()
                             .Be("One or more validation errors occurred.");
            errorObject.Errors.Should()
                              .NotBeEmpty();

            errorObject.Errors.ContainsKey("page").Should().Be(page <= 0, "page <= 0 is not a valid value");
            errorObject.Errors.ContainsKey("pageSize").Should().Be(pageSize <= 0, "pageSize <= 0 is not a valid value");
        }

        public static IEnumerable<object[]> InvalidSearchCases
        {
            get
            {
                yield return new object[]
                {
                    "?page=-1" ,
                    (Expression<Func<ValidationProblemDetails, bool>>)(err => err.Status == Status400BadRequest
                        && err.Title == "One or more validation errors occurred."
                        && err.Errors != null
                        && err.Errors.Once(kv => kv.Key.Equals("page", StringComparison.OrdinalIgnoreCase))
                    ),
                    $"{nameof(SearchAppointmentModel.Page)} must be greater than 1"
                };

                yield return new object[]
                {
                    "?page=1&pageSize=-1" ,
                    (Expression<Func<ValidationProblemDetails, bool>>)(err => err.Status == Status400BadRequest
                        && err.Title == "One or more validation errors occurred."
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

            string url = $"{_endpointUrl}/{nameof(AppointmentsController.Search)}{queryString}";
            _outputHelper.WriteLine($"Url under test : <{url}>");

            HttpRequestMessage request = new(HttpMethod.Head, url);

            // Arrange
            url = $"{_endpointUrl}/{Guid.Empty}";
            _outputHelper.WriteLine($"Requested url : <{url}>");

            IEnumerable<Claim> claims = new[]
            {
                new Claim(ClaimTypes.Name, "Bruce Wayne")
            };
            using HttpClient client = _sut.CreateClient();

            // Act
            using HttpResponseMessage response = await client.SendAsync(request)
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
            // Arrange
            string url = $"{_endpointUrl}/search?sort={Uri.EscapeDataString("+startDate")}";

            HttpRequestMessage request = new(new HttpMethod(verb), url);

            IEnumerable<Claim> claims = new[]
            {
                new Claim(ClaimTypes.Name, "Bruce Wayne")
            };
            using HttpClient client = _sut.CreateClient();

            // Act
            using HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);

            string content = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"Response content : {content}");

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
                    "?page=1&pageSize=10",
                };

                yield return new object[]
                {
                    $"/search?{new { page=1, pageSize=10, from = new LocalDate(2019, 10, 12)} }",
                };
            }
        }

        [Theory]
        [MemberData(nameof(GetCountCases))]
        public async Task Given_a_head_request_that_target_a_endpoint_that_return_a_collection_Response_should_contain_count_headers(string url)
        {
            // Arrange
            string path = $"{_endpointUrl}{url}";
            _outputHelper.WriteLine($"path under test : {path}");
            HttpRequestMessage request = new(HttpMethod.Head, path);
            IEnumerable<Claim> claims = new[]
            {
                new Claim(ClaimTypes.Name, "Bruce Wayne")
            };
            using HttpClient client = _sut.CreateClient();
            client.DefaultRequestHeaders.Add("api-version", "1.0");

            // Act
            using HttpResponseMessage response = await client.SendAsync(request)
                                                       .ConfigureAwait(false);

            // Assert
            _outputHelper.WriteLine($"Response status code : {response.StatusCode}");
            _outputHelper.WriteLine($"Response headers :{response.Headers.Jsonify()}");

            response.IsSuccessStatusCode.Should().BeTrue();

            response.Headers.Should()
                            .ContainSingle(header => header.Key == AddCountHeadersFilterAttribute.TotalCountHeaderName).And
                            .ContainSingle(header => header.Key == AddCountHeadersFilterAttribute.CountHeaderName);

            response.Headers.GetValues(AddCountHeadersFilterAttribute.TotalCountHeaderName).Should()
                .HaveCount(1, $"{AddCountHeadersFilterAttribute.TotalCountHeaderName} must contain only a single value");

            response.Headers.GetValues(AddCountHeadersFilterAttribute.CountHeaderName).Should()
                .HaveCount(1, $"{AddCountHeadersFilterAttribute.CountHeaderName} must contain only a single value");
        }

        [Fact]
        public async Task When_posting_valid_data_post_create_the_resource()
        {
            // Arrange
            Faker<AttendeeModel> participantFaker = new Faker<AttendeeModel>()
                .RuleFor(x => x.Id, () => AttendeeId.New())
                .RuleFor(x => x.Name, faker => faker.Name.FullName())
                .RuleFor(x => x.UpdatedDate, faker => faker.Noda().Instant.Recent());

            Faker<NewAppointmentModel> appointmentFaker = new Faker<NewAppointmentModel>("en")
                .RuleFor(x => x.Attendees, participantFaker.Generate(count: 3))
                .RuleFor(x => x.Location, faker => faker.Address.City())
                .RuleFor(x => x.Subject, faker => faker.Lorem.Sentence())
                .RuleFor(x => x.StartDate, faker => faker.Noda().ZonedDateTime.Future(reference: 1.January(DateTime.UtcNow.Year + 1).Add(1.Hours()).AsUtc().ToInstant().InUtc()))
                .RuleFor(x => x.EndDate, (_, appointment) => appointment.StartDate + 1.Hours().ToDuration());

            NewAppointmentModel newAppointment = appointmentFaker.Generate();

            _outputHelper.WriteLine($"{nameof(newAppointment)} : {newAppointment.Jsonify(SerializerOptions)}");
            _outputHelper.WriteLine($"Url : {_endpointUrl}");

            IEnumerable<Claim> claims = new[]
            {
                new Claim(ClaimTypes.Name, "Bruce Wayne")
            };
            using HttpClient client = _sut.CreateClient();
            client.DefaultRequestHeaders.Add("api-version", "1.0");

            // Act
            HttpResponseMessage response = await client.PostAsJsonAsync(_endpointUrl, newAppointment, SerializerOptions)
                                                       .ConfigureAwait(false);

            // Assert
            _outputHelper.WriteLine($"Response Status code :  {response.StatusCode}");
            response.IsSuccessStatusCode.Should()
                .BeTrue("The resource creation must succeed");
            ((int)response.StatusCode).Should().Be(Status201Created);

            Browsable<AppointmentModel> browsableResource = await response.Content.ReadFromJsonAsync<Browsable<AppointmentModel>>(SerializerOptions)
                                                                                  .ConfigureAwait(false);

            IEnumerable<Link> links = browsableResource.Links;
            links.Should()
                 .ContainSingle(link => link.Relation == Self);

            Uri location = response.Headers.Location;

            response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, location))
                                   .ConfigureAwait(false);

            response.IsSuccessStatusCode.Should()
                                        .BeTrue($"location <{location}> must point to the created resource");

            // Cleanup
            await client.DeleteAsync(location)
                        .ConfigureAwait(false);
        }

        [Fact]
        public async Task Given_attendee_exists_on_appointment_DeleteAttendee_should_complete()
        {
            // Arrange
            Faker<AttendeeModel> participantFaker = new Faker<AttendeeModel>("en")
                        .RuleFor(x => x.Name, faker => faker.Name.FullName())
                        .RuleFor(x => x.Id, () => AttendeeId.New());

            NewAppointmentModel newAppointmentModel = new Faker<NewAppointmentModel>("en")
                .RuleFor(x => x.Attendees, (faker) => participantFaker.Generate(faker.Random.Int(min: 2, max: 5)))
                .RuleFor(x => x.Location, faker => faker.Address.City())
                .RuleFor(x => x.Subject, faker => faker.Lorem.Sentence())
                .RuleFor(x => x.StartDate, faker => faker.Noda().Instant.Future(reference: 1.January(DateTime.UtcNow.Year + 1).Add(1.Hours()).AsUtc().ToInstant()).InUtc())
                .RuleFor(x => x.EndDate, (_, appointment) => appointment.StartDate + 1.Hours().ToDuration());

            IEnumerable<Claim> claims = new[]
            {
                new Claim(ClaimTypes.Name, "Bruce Wayne")
            };
            using HttpClient client = _sut.CreateClient();
            client.DefaultRequestHeaders.Add("api-version", "1");

            HttpResponseMessage response = await client.PostAsJsonAsync(_endpointUrl, newAppointmentModel, SerializerOptions)
                                                       .ConfigureAwait(false);

            _outputHelper.WriteLine($"Creation of the appointment for the integration test response status : {response.StatusCode}");
            Browsable<AppointmentModel> browsableAppointment = await response.Content.ReadFromJsonAsync<Browsable<AppointmentModel>>(SerializerOptions)
                                                                                     .ConfigureAwait(false);
            AppointmentModel resource = browsableAppointment.Resource;
            IEnumerable<AttendeeModel> attendees = resource.Attendees;

            AttendeeModel attendeeToDelete = new Faker().PickRandom(attendees);

            AppointmentId appointmentId = resource.Id;
            AttendeeId attendeeId = attendeeToDelete.Id;
            string deletePath = $"{_endpointUrl}/{appointmentId}/attendees/{attendeeId}";

            _outputHelper.WriteLine($"delete url : <{deletePath}>");

            // Act
            response = await client.DeleteAsync(deletePath)
                                   .ConfigureAwait(false);

            // Assert
            _outputHelper.WriteLine("Starting assertions");
            _outputHelper.WriteLine($"Delete status code : {response.StatusCode}");

            response.IsSuccessStatusCode.Should()
                .BeTrue($"Appointment <{appointmentId}> and Attendee <{attendeeId}> exist.");

            ((int)response.StatusCode).Should()
                                      .Be(Status204NoContent);
        }
    }
}
