using Agenda.DataStores;
using FluentAssertions;
using MedEasy.DAL.Context;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;
using MedEasy.RestObjects;
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
    public class AppointmentsControllerTests : IClassFixture<ServicesTestFixture<Startup>>, IDisposable
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

        public AppointmentsControllerTests(ITestOutputHelper outputHelper, ServicesTestFixture<Startup> fixture)
        {
            _outputHelper = outputHelper;
            fixture.Initialize(
                relativeTargetProjectParentDir: Path.Combine("..", "..", "..", "..", "src", "services", "Agenda"),
                environmentName: "IntegrationTest",
                applicationName: "Agenda.API",
                initializeServices: (services) => services.AddSingleton<IUnitOfWorkFactory, EFUnitOfWorkFactory<AgendaContext>>(item =>
                {
                    DbContextOptionsBuilder<AgendaContext> builder = new DbContextOptionsBuilder<AgendaContext>();
                    builder.UseInMemoryDatabase($"{Guid.NewGuid()}");

                    return new EFUnitOfWorkFactory<AgendaContext>(builder.Options, (options) =>
                    {
                        AgendaContext context = new AgendaContext(options);
                        return context;
                    });

                })
            );
            _server = fixture.Server;
        }


        public void Dispose()
        {
            _outputHelper = null;
            _server.Dispose();

        }


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


    }
}
