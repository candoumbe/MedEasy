using Agenda.API.Controllers;
using Agenda.DTO;
using Agenda.DTO.Resources.Search;
using Bogus;
using FluentAssertions;
using FluentAssertions.Extensions;
using MedEasy.Core.Filters;
using MedEasy.IntegrationTests.Core;
using MedEasy.RestObjects;
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

namespace Agenda.API.IntegrationTests
{
    [IntegrationTest]
    [Feature("Agenda")]
    [Feature("Appointments")]
    public class AppointmentsControllerTests : IClassFixture<IntegrationFixture<Startup>>
    {
        private IntegrationFixture<Startup> _server;
        private ITestOutputHelper _outputHelper;
        private const string _endpointUrl = "/agenda/appointments";

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
        /// Schema of an <see cref="AppointmentInfo"/> resource once translated to json
        /// </summary>
        private static readonly JSchema _appointmentResourceSchema = new JSchema
        {
            Type = JSchemaType.Object,
            Properties =
            {
                [nameof(AppointmentInfo.Id).ToCamelCase()] = new JSchema { Type = JSchemaType.String },
                [nameof(AppointmentInfo.Subject).ToCamelCase()] = new JSchema { Type = JSchemaType.String },
                [nameof(AppointmentInfo.Location).ToCamelCase()] = new JSchema { Type = JSchemaType.String },
                [nameof(AppointmentInfo.StartDate).ToCamelCase()] = new JSchema { Type = JSchemaType.String,  },
                [nameof(AppointmentInfo.EndDate).ToCamelCase()] = new JSchema { Type = JSchemaType.String,  },
                [nameof(AppointmentInfo.UpdatedDate).ToCamelCase()] = new JSchema { Type = JSchemaType.String, },
                [nameof(AppointmentInfo.Participants).ToCamelCase()] = new JSchema { Type = JSchemaType.Array,  MinimumItems = 1}
            },
            Required =
            {
                nameof(AppointmentInfo.Id).ToCamelCase(),
                nameof(AppointmentInfo.Subject).ToCamelCase(),
                nameof(AppointmentInfo.Location).ToCamelCase(),
                nameof(AppointmentInfo.StartDate).ToCamelCase(),
                nameof(AppointmentInfo.EndDate).ToCamelCase(),
                nameof(AppointmentInfo.Participants).ToCamelCase(),

            }
        };

        private static readonly JSchema _browsableResourceSchema = new JSchema
        {
            Type = JSchemaType.Object,
            Properties =
            {
                [nameof(Browsable<AppointmentInfo>.Resource).ToLower()] = _appointmentResourceSchema,
                [nameof(Browsable<AppointmentInfo>.Links).ToLower()] = new JSchema
                {
                    Type = JSchemaType.Array,
                }

            }
        };

        public AppointmentsControllerTests(ITestOutputHelper outputHelper, IntegrationFixture<Startup> fixture)
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


            // Act
            using (HttpClient client = _server.CreateClient())
            {
                HttpResponseMessage response = await client.GetAsync(url)
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
                    .Be("Validation failed");
                errorObject.Errors.Should()
                    .NotBeEmpty();

                if (page <= 0)
                {
                    errorObject.Errors.ContainsKey("page").Should().BeTrue("page <= 0 is not a valid value");
                }

                if (pageSize <= 0)
                {
                    errorObject.Errors.ContainsKey("pageSize").Should().BeTrue("pageSize <= 0 is not a valid value");
                }
            }
        }

        public static IEnumerable<object[]> InvalidSearchCases
        {
            get
            {
                yield return new object[]
                {
                    "?page=-1" ,
                    ((Expression<Func<ValidationProblemDetails, bool>>)(err => err.Status == Status400BadRequest
                        && err.Title == "Validation failed"
                        && err.Errors != null
                        && err.Errors.ContainsKey("page")
                    )),
                    $"{nameof(SearchAppointmentInfo.Page)} must be greater than 1"
                };

                yield return new object[]
                {
                    "?pageSize=-1" ,
                    (Expression<Func<ValidationProblemDetails, bool>>)(err => err.Status == Status400BadRequest
                        && err.Title == "Validation failed"
                        && err.Errors != null
                        && err.Errors.ContainsKey("pageSize")
                    ),
                    $"{nameof(SearchAppointmentInfo.PageSize)} must be greater than 1"
                };
            }
        }

        [Theory]
        [MemberData(nameof(InvalidSearchCases))]
        public async Task GivenInvalidCriteria_Search_Returns_BadRequest(string queryString, Expression<Func<ValidationProblemDetails, bool>> errorObjectExpectation, string reason)
        {
            _outputHelper.WriteLine($"search query string : {queryString}");

            // Arrange
            string url = $"{_endpointUrl}/{nameof(AppointmentsController.Search)}{queryString}";
            _outputHelper.WriteLine($"Url under test : <{url}>");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, url);

            // Act
            using (HttpClient client = _server.CreateClient())
            {
                HttpResponseMessage response = await client.SendAsync(request)
                        .ConfigureAwait(false);

                // Assert
                response.IsSuccessStatusCode.Should().BeFalse("Invalid search criteria");
                ((int)response.StatusCode).Should().Be(Status400BadRequest);

                string content = await response.Content.ReadAsStringAsync()
                    .ConfigureAwait(false);

                _outputHelper.WriteLine($"Response content : {content}");
                content.Should()
                    .NotBeNullOrEmpty("BAD REQUEST content must provide additional information on errors");

                JToken token = JToken.Parse(content);
                token.IsValid(_errorObjectSchema)
                    .Should().BeTrue($"Error object must be provided when HTTP GET <{url}> returns BAD REQUEST");

                ValidationProblemDetails errorObject = token.ToObject<ValidationProblemDetails>();
                errorObject.Should().Match(errorObjectExpectation, reason);
            }
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("HEAD")]
        public async Task Search_Handles_Verb(string verb)
        {
            // Arrange
            string url = $"{_endpointUrl}/search?sort={Uri.EscapeDataString("+startDate")}";

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(verb), url);

            using (HttpClient client = _server.CreateClient())
            {
                // Act
                HttpResponseMessage response = await client.SendAsync(request)
                    .ConfigureAwait(false);

                string content = await response.Content.ReadAsStringAsync()
                    .ConfigureAwait(false);

                _outputHelper.WriteLine($"Response content : {content}");

                // Assert
                response.IsSuccessStatusCode.Should()
                    .BeTrue($"HTTP {response.Version} {verb} /{url} must be supported");
            }
        }

        public static IEnumerable<object[]> GetCountCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<NewAppointmentInfo>(),
                    "?page=1&pageSize=10",
                    (total : 0, count : 0)
                };

                {
                    Faker<ParticipantInfo> participantFaker = new Faker<ParticipantInfo>("en")
                        .RuleFor(x => x.Name, faker => faker.Name.FullName());

                    Faker<NewAppointmentInfo> appointmentFaker = new Faker<NewAppointmentInfo>("en")
                        .RuleFor(x => x.Participants, (faker) => participantFaker.Generate(faker.Random.Int(min: 1, max: 5)))
                        .RuleFor(x => x.Location, faker => faker.Address.City())
                        .RuleFor(x => x.Subject, faker => faker.Lorem.Sentence())
                        .RuleFor(x => x.StartDate, faker => faker.Date.Future(refDate: 1.January(DateTimeOffset.UtcNow.Year + 1).Add(1.Hours())))
                        .RuleFor(x => x.EndDate, (_, appointment) => appointment.StartDate.Add(1.Hours()));

                    yield return new object[]
                    {
                        appointmentFaker.Generate(count : 10),
                        $"/search?{new { page=1, pageSize=10, from = 1.January(DateTimeOffset.UtcNow.Year)}.ToQueryString()}",
                        (total : 10, count : 10)
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetCountCases))]
        public async Task Enpoint_Provides_CountsHeaders(IEnumerable<NewAppointmentInfo> newAppointments, string url, (int total, int count) expectedCountHeaders)
        {
            // Arrange
            _outputHelper.WriteLine($"Nb items to create : {newAppointments.Count()}");
            using (HttpClient client = _server.CreateClient())
            {
                await newAppointments.ForEachAsync(async (newAppointment) =>
                {
                    HttpResponseMessage createdResponse = await client.PostAsync(_endpointUrl, new StringContent(newAppointment.Stringify(), Encoding.UTF8, "application/json"))
                    .ConfigureAwait(false);

                    _outputHelper.WriteLine($"{nameof(createdResponse)} status : {createdResponse.StatusCode}");
                })
                .ConfigureAwait(false);
            }

            string path = $"{_endpointUrl}{url}";
            _outputHelper.WriteLine($"path under test : {path}");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, path);

            using (HttpClient client = _server.CreateClient())
            {
                // Act
                HttpResponseMessage response = await client.SendAsync(request)
                    .ConfigureAwait(false);

                // Assert
                _outputHelper.WriteLine($"Response content : {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");
                response.IsSuccessStatusCode.Should().BeTrue();

                _outputHelper.WriteLine($"Response status code : {response.StatusCode}");
                response.IsSuccessStatusCode.Should().BeTrue();

                _outputHelper.WriteLine($"Response headers :{response.Headers.Stringify()}");

                response.Headers.Should()
                    .ContainSingle(header => header.Key == AddCountHeadersFilterAttribute.TotalCountHeaderName).And
                    .ContainSingle(header => header.Key == AddCountHeadersFilterAttribute.CountHeaderName);

                response.Headers.GetValues(AddCountHeadersFilterAttribute.TotalCountHeaderName).Should()
                    .HaveCount(1).And
                    .ContainSingle().And
                    .ContainSingle(value => value == expectedCountHeaders.total.ToString());

                response.Headers.GetValues(AddCountHeadersFilterAttribute.CountHeaderName).Should()
                    .HaveCount(1).And
                    .ContainSingle().And
                    .ContainSingle(value => value == expectedCountHeaders.count.ToString());
            }
        }

        [Fact]
        public async Task WhenPostingValidData_Post_CreateTheResource()
        {
            // Arrange

            Faker<ParticipantInfo> participantFaker = new Faker<ParticipantInfo>()
                .RuleFor(x => x.Name, faker => faker.Name.FullName())
                .RuleFor(x => x.UpdatedDate, faker => faker.Date.Recent());

            Faker<NewAppointmentInfo> appointmentFaker = new Faker<NewAppointmentInfo>("en")
                .RuleFor(x => x.Participants, participantFaker.Generate(count: 3))
                .RuleFor(x => x.Location, faker => faker.Address.City())
                .RuleFor(x => x.Subject, faker => faker.Lorem.Sentence())
                .RuleFor(x => x.StartDate, faker => faker.Date.Future(refDate: 1.January(DateTimeOffset.UtcNow.Year + 1).Add(1.Hours())))
                .RuleFor(x => x.EndDate, (faker, appointment) => appointment.StartDate.Add(1.Hours()));

            NewAppointmentInfo newAppointment = appointmentFaker.Generate();

            _outputHelper.WriteLine($"{nameof(newAppointment)} : {newAppointment.Stringify()}");

            // Act
            using (HttpClient client = _server.CreateClient())
            {
                HttpResponseMessage response = await client.PostAsync(_endpointUrl, new StringContent(SerializeObject(newAppointment), Encoding.UTF8, "application/json"))
                    .ConfigureAwait(false);

                // Assert
                string json = await response.Content.ReadAsStringAsync()
                    .ConfigureAwait(false);

                _outputHelper.WriteLine($"content : {json}");

                _outputHelper.WriteLine($"Response Status code :  {response.StatusCode}");
                response.IsSuccessStatusCode.Should()
                    .BeTrue("The resource creation must succeed");
                ((int)response.StatusCode).Should().Be(Status201Created);

                response.Content.Should()
                    .NotBeNull("API must return a content");

                json.Should()
                    .NotBeNullOrWhiteSpace();

                JToken token = JToken.Parse(json);
                bool tokenIsValid = token.IsValid(_browsableResourceSchema, out IList<string> errors);
                tokenIsValid.Should()
                    .BeTrue("content returned by the API must conform to appointment jschema");

                Browsable<AppointmentInfo> browsableResource = token.ToObject<Browsable<AppointmentInfo>>();
                IEnumerable<Link> links = browsableResource.Links;
                Link linkToSelf = links.Single(link => link.Relation == LinkRelation.Self);

                Uri location = response.Headers.Location;
                linkToSelf.Href.Should().Be(location.ToString());

                response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, location))
                    .ConfigureAwait(false);

                response.IsSuccessStatusCode.Should()
                    .BeTrue($"location <{location}> must point to the created resource");

            }
        }

        [Fact]
        public async Task RemoveParticipantFromAppointment_MustComplete()
        {
            // Arrange
            Faker<ParticipantInfo> participantFaker = new Faker<ParticipantInfo>("en")
                        .RuleFor(x => x.Name, faker => faker.Name.FullName())
                        .RuleFor(x => x.Id, () => Guid.NewGuid());

            Faker<NewAppointmentInfo> appointmentFaker = new Faker<NewAppointmentInfo>("en")
                .RuleFor(x => x.Participants, (faker) => participantFaker.Generate(faker.Random.Int(min: 2, max: 5)))
                .RuleFor(x => x.Location, faker => faker.Address.City())
                .RuleFor(x => x.Subject, faker => faker.Lorem.Sentence())
                .RuleFor(x => x.StartDate, faker => faker.Date.Future(refDate: 1.January(DateTimeOffset.UtcNow.Year + 1).Add(1.Hours())))
                .RuleFor(x => x.EndDate, (faker, appointment) => appointment.StartDate.Add(1.Hours()));

            NewAppointmentInfo newAppointmentInfo = appointmentFaker;


            using (HttpClient client = _server.CreateClient())
            {
                HttpResponseMessage response = await client.PostAsync(_endpointUrl, new StringContent(newAppointmentInfo.Stringify(), Encoding.UTF8, "application/json"))
                        .ConfigureAwait(false);

                _outputHelper.WriteLine($"Creation of the appointment for the integration test response stats : {response.StatusCode}");
                string json = await response.Content.ReadAsStringAsync()
                    .ConfigureAwait(false);

                _outputHelper.WriteLine($"Response content : {json}");

                Browsable<AppointmentInfo> browsableAppointmentInfo = JToken.Parse(json).ToObject<Browsable<AppointmentInfo>>();

                _outputHelper.WriteLine($"Resource created : {browsableAppointmentInfo.Stringify()}");

                AppointmentInfo resource = browsableAppointmentInfo.Resource;
                IEnumerable<ParticipantInfo> participants = resource.Participants;
                int participantCountBeforeDelete = participants.Count();

                ParticipantInfo participantToDelete = new Faker().PickRandom(participants);

                Guid appointmentId = resource.Id;
                Guid participantId = participantToDelete.Id;
                string deletePath = $"{_endpointUrl}/{appointmentId}/participants/{participantId}";

                _outputHelper.WriteLine($"delete url : <{deletePath}>");

                // Act
                response = await client.DeleteAsync(deletePath)
                    .ConfigureAwait(false);

                // Assert
                _outputHelper.WriteLine("Starting assertions");
                _outputHelper.WriteLine($"Delete status code : {response.StatusCode}");

                response.IsSuccessStatusCode.Should()
                    .BeTrue($"Appointment <{appointmentId}> and Participant <{participantId}> exist.");

                ((int)response.StatusCode).Should()
                    .Be(Status204NoContent);
            }
        }
    }
}
