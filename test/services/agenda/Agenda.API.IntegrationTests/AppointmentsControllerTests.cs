using Agenda.API.Controllers;
using Agenda.DataStores;
using Agenda.DTO;
using Agenda.DTO.Resources.Search;
using Bogus;
using FluentAssertions;
using FluentAssertions.Extensions;
using MedEasy.Core.Filters;
using MedEasy.DAL.Context;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Microsoft.AspNetCore.Http.HttpMethods;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Newtonsoft.Json.JsonConvert;


namespace Agenda.API.IntegrationTests
{
    [IntegrationTest]
    [Feature("Agenda")]
    [Feature("Appointments")]
    public class AppointmentsControllerTests : IClassFixture<ServicesTestFixture<Startup>>, IDisposable, IClassFixture<DatabaseFixture>
    {
        private TestServer _server;
        private ITestOutputHelper _outputHelper;
        private const string _endpointUrl = "/agenda/appointments";
        private static JSchema _errorObjectSchema = new JSchema
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
        private static JSchema _pageLink = new JSchema
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

        private static JSchema _pageResponseSchema = new JSchema
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
        /// <summary>
        /// Schema of an <see cref="AppointmentInfo"/> resource once translated to json
        /// </summary>
        private static JSchema _appointmentResourceSchema = new JSchema
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

        private static JSchema _browsableResourceSchema = new JSchema
        {
            Type = JSchemaType.Object,
            Properties =
            {
                [nameof(BrowsableResource<AppointmentInfo>.Resource).ToLower()] = _appointmentResourceSchema,
                [nameof(BrowsableResource<AppointmentInfo>.Links).ToLower()] = new JSchema
                {
                    Type = JSchemaType.Array,
                }

            }
        };

        private DatabaseFacade _databaseFacade;

        public AppointmentsControllerTests(ITestOutputHelper outputHelper, ServicesTestFixture<Startup> fixture, DatabaseFixture database)
        {
            _outputHelper = outputHelper;
            fixture.Initialize(
                relativeTargetProjectParentDir: Path.Combine("..", "..", "..", "..", "src", "services", "Agenda"),
                environmentName: "IntegrationTest",
                applicationName: typeof(Startup).Assembly.GetName().Name,
                initializeServices: (services) => services.AddSingleton<IUnitOfWorkFactory, EFUnitOfWorkFactory<AgendaContext>>(item =>
                {
                    DbContextOptionsBuilder<AgendaContext> builder = new DbContextOptionsBuilder<AgendaContext>();
                    builder.UseSqlite(database.Connection)
                        .EnableSensitiveDataLogging()
                        .ConfigureWarnings(warnings => warnings.Throw());

                    return new EFUnitOfWorkFactory<AgendaContext>(builder.Options, (options) =>
                    {
                        AgendaContext context = new AgendaContext(options);
                        _databaseFacade = context.Database;
                        _databaseFacade.EnsureCreated();
                        return context;
                    });

                })
            );
            _server = fixture.Server;
        }


        public void Dispose()
        {
            _outputHelper = null;
            _databaseFacade?.EnsureDeleted();
            _server.Dispose();

        }

        private static string Stringify(object o) => SerializeObject(o, new JsonSerializerSettings { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

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
            string url = $"{_endpointUrl}?page={page}&pageSize={pageSize}";
            _outputHelper.WriteLine($"Url under test : <{url}>");
            RequestBuilder rb = _server.CreateRequest(url)
                .AddHeader("Accept", "application/json");

            // Act
            HttpResponseMessage response = await rb.GetAsync()
                .ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should().BeFalse("Invalid page and/or pageSize");
            ((int)response.StatusCode).Should().Be(Status400BadRequest);

            string content = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"Response content : {content}");

            content.Should()
                .NotBeNullOrEmpty("BAD REQUEST content must provide additional information on errors");

            JToken token = JToken.Parse(content);
            token.IsValid(_errorObjectSchema)
                .Should().BeTrue("Error object must be provided when API returns BAD REQUEST");

            ErrorObject errorObject = token.ToObject<ErrorObject>();
            errorObject.Code.Should()
                .Be("BAD_REQUEST");
            errorObject.Description.Should()
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


        public static IEnumerable<object[]> InvalidSearchCases
        {
            get
            {
                yield return new object[]
                {
                    "?page=-1" ,
                    ((Expression<Func<ErrorObject, bool>>)(err => err.Code == "BAD_REQUEST"
                        && err.Description == "Validation failed"
                        && err.Errors != null
                        && err.Errors.ContainsKey("page")
                    )),
                    $"{nameof(SearchAppointmentInfo.Page)} must be greater than 1"
                };

                yield return new object[]
                {
                    "?pageSize=-1" ,
                    ((Expression<Func<ErrorObject, bool>>)(err => err.Code == "BAD_REQUEST"
                        && err.Description == "Validation failed"
                        && err.Errors != null
                        && err.Errors.ContainsKey("pageSize")
                    )),
                    $"{nameof(SearchAppointmentInfo.PageSize)} must be greater than 1"
                };
            }
        }

        [Theory]
        [MemberData(nameof(InvalidSearchCases))]
        public async Task GivenInvalidCriteria_Search_Returns_BadRequest(string queryString, Expression<Func<ErrorObject, bool>> errorObjectExpectation, string reason)
        {
            _outputHelper.WriteLine($"search query string : {queryString}");

            // Arrange
            string url = $"{_endpointUrl}/{nameof(AppointmentsController.Search)}{queryString}";
            _outputHelper.WriteLine($"Url under test : <{url}>");
            RequestBuilder rb = _server.CreateRequest(url)
                .AddHeader("Accept", "application/json");

            // Act
            HttpResponseMessage response = await rb.SendAsync(Get)
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

            ErrorObject errorObject = token.ToObject<ErrorObject>();

            errorObject.Should().Match(errorObjectExpectation, reason);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("HEAD")]
        public async Task Search_Handles_Verb(string verb)
        {

            // Arrange
            string url = $"{_endpointUrl}/search?sort=+startDate";
            RequestBuilder requestBuilder = new RequestBuilder(_server, url)
                .AddHeader("Accept", "application/json");


            // Act
            HttpResponseMessage response = await requestBuilder.SendAsync(verb)
                .ConfigureAwait(false);


            // Assert
            response.IsSuccessStatusCode.Should()
                .BeTrue($@"HTTP {response.Version} {verb} /{url} must be supported");
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
                        .RuleFor(x => x.EndDate, (faker, appointment) => appointment.StartDate.Add(1.Hours()));
                    
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
        public async Task Enpoint_Provides_CountsHeaders(IEnumerable<NewAppointmentInfo> newAppointments, string url, (int total, int count) countHeaders)
        {
            // Arrange
            _outputHelper.WriteLine($"Nb items to create : {newAppointments.Count()}");
            await newAppointments.ForEachAsync(async (newAppointment) =>
            {
                RequestBuilder rb = new RequestBuilder(_server, _endpointUrl)
                    .And(message => message.Content = new StringContent(SerializeObject(newAppointment), Encoding.UTF8, "application/json"));

                HttpResponseMessage createdResponse = await rb.PostAsync()
                    .ConfigureAwait(false);

                _outputHelper.WriteLine($"{nameof(createdResponse)} status : {createdResponse.StatusCode}");
                
            })
            .ConfigureAwait(false);
            

            string path = $"{_endpointUrl}{url}";
            _outputHelper.WriteLine($"path under test : {path}");
            RequestBuilder requestBuilder = new RequestBuilder(_server, path);

            // Act
            HttpResponseMessage response = await requestBuilder.SendAsync(Head)
                .ConfigureAwait(false);

            // Assert
            _outputHelper.WriteLine($"Response status code : {response.StatusCode}");
            response.IsSuccessStatusCode.Should().BeTrue();

            _outputHelper.WriteLine($"Response headers :{Stringify(response.Headers)}");

            response.Headers.Should()
                .ContainSingle(header => header.Key == AddCountHeadersFilterAttribute.TotalCountHeaderName).And
                .ContainSingle(header => header.Key == AddCountHeadersFilterAttribute.CountHeaderName);

            response.Headers.GetValues(AddCountHeadersFilterAttribute.TotalCountHeaderName).Should()
                .HaveCount(1).And
                .ContainSingle().And
                .ContainSingle(value => value == countHeaders.total.ToString());

            response.Headers.GetValues(AddCountHeadersFilterAttribute.CountHeaderName).Should()
                .HaveCount(1).And
                .ContainSingle().And
                .ContainSingle(value => value == countHeaders.count.ToString());

        }

        [Fact]
        public async Task WhenPostingValidData_Post_CreateTheResource()
        {
            // Arrange
            Faker<ParticipantInfo> participantFaker = new Faker<ParticipantInfo>("en")
                        .RuleFor(x => x.Name, faker => faker.Person.FullName);

            Faker<NewAppointmentInfo> appointmentFaker = new Faker<NewAppointmentInfo>("en")
                .RuleFor(x => x.Participants, (faker) => participantFaker.Generate(count : 3))
                .RuleFor(x => x.Location, faker => faker.Address.City())
                .RuleFor(x => x.Subject, faker => faker.Lorem.Sentence())
                .RuleFor(x => x.StartDate, faker => faker.Date.Future(refDate: 1.January(DateTimeOffset.UtcNow.Year + 1).Add(1.Hours())))
                .RuleFor(x => x.EndDate, (faker, appointment) => appointment.StartDate.Add(1.Hours()));

            NewAppointmentInfo newAppointment = appointmentFaker.Generate();

            _outputHelper.WriteLine($"{nameof(newAppointment)} : {Stringify(newAppointment)}");

            RequestBuilder requestBuilder = new RequestBuilder(_server, _endpointUrl)
                .AddHeader("Accept", "application/json")
                .And(message => message.Content = new StringContent(SerializeObject(newAppointment), Encoding.UTF8, "application/json"));

            // Act
            HttpResponseMessage response = await requestBuilder.PostAsync()
                .ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should()
                .BeTrue("The resource creation must succeed");
            ((int)response.StatusCode).Should().Be(Status201Created);

            response.Content.Should()
                .NotBeNull("API must return a content");

            string json = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"content : {json}");

            json.Should()
                .NotBeNullOrWhiteSpace();


            JToken token = JToken.Parse(json);
            bool tokenIsValid = token.IsValid(_browsableResourceSchema, out IList<string> errors);
            tokenIsValid.Should()
                .BeTrue("content returned by the API must conform to appointment jschema");

            BrowsableResource<AppointmentInfo> browsableResource = token.ToObject<BrowsableResource<AppointmentInfo>>();
            IEnumerable<Link> links = browsableResource.Links;
            Link linkToSelf = links.Single(link => link.Relation == LinkRelation.Self);

            Uri location = response.Headers.Location;
            linkToSelf.Href.Should().Be(location.ToString());

            response = await new RequestBuilder(_server, $"{location}").SendAsync(Head)
                .ConfigureAwait(false);

            response.IsSuccessStatusCode.Should()
                .BeTrue($"location <{location}> must point to the created resource");

        }
    }
}
