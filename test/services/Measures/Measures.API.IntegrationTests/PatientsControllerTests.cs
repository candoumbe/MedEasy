using FluentAssertions;
using Measures.API.Context;
using Measures.DTO;
using MedEasy.DAL.Context;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Measures.API.IntegrationTests
{
    [Collection("IntegrationTests")]
    public class PatientsControllerTests : IDisposable, IClassFixture<ServicesTestFixture<Startup>>
    {
        private TestServer _server;
        private ITestOutputHelper _outputHelper;

        public PatientsControllerTests(ITestOutputHelper outputHelper, ServicesTestFixture<Startup> fixture)
        {
            _outputHelper = outputHelper;
            fixture.Initialize(
                relativeTargetProjectParentDir : Path.Combine("..", "..", "..", "..", "src", "services", "Measures"),
                environmentName: "Development", 
                applicationName: "Measures.API",
                initializeServices: (services) => services.AddSingleton<IUnitOfWorkFactory, EFUnitOfWorkFactory<MeasuresContext>>(item =>
                {
                    DbContextOptionsBuilder<MeasuresContext> builder = new DbContextOptionsBuilder<MeasuresContext>();
                    builder.UseInMemoryDatabase($"{Guid.NewGuid()}");

                    return new EFUnitOfWorkFactory<MeasuresContext>(builder.Options, (options) => new MeasuresContext(options));

                })
            );
            _server = fixture.Server;
        }


        public void Dispose()
        {
            _outputHelper = null;
            _server.Dispose();

        }

        [Fact]
        public async Task GetAll_With_No_Data()
        {
            // Arrange

            JSchema pageLink = new JSchema
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

            JSchema pageResponseSchema = new JSchema
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
                            [nameof(PagedRestResponseLink.First).ToLower()] = pageLink,
                            [nameof(PagedRestResponseLink.Previous).ToLower()] = pageLink,
                            [nameof(PagedRestResponseLink.Next).ToLower()] = pageLink,
                            [nameof(PagedRestResponseLink.Last).ToLower()] = pageLink
                        },
                        Required =
                        {
                            nameof(PagedRestResponseLink.First).ToLower(),
                            nameof(PagedRestResponseLink.Last).ToLower()
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

            RequestBuilder rb = _server.CreateRequest("/measures/patients")
                .AddHeader("Accept", "application/json")
                .AddHeader("Accept-Charset", "uf-8");

            // Act
            HttpResponseMessage response = await rb.GetAsync()
                .ConfigureAwait(false);

            // Assert
            ((int)response.StatusCode).Should().Be(StatusCodes.Status200OK);
            HttpContentHeaders headers = response.Content.Headers;


            string json = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"json : {json}");

            JToken jToken = JToken.Parse(json);
            jToken.IsValid(pageResponseSchema).Should().BeTrue();
        }

        

        [Theory]
        [InlineData("/measures/patients", "GET")]
        [InlineData("/measures/patients", "HEAD")]
        [InlineData("/measures/patients", "OPTIONS")]
        public async Task ShouldReturnsSuccessCode(string url, string method)
        {

            // Arrange
            RequestBuilder rb = _server.CreateRequest(url);

            // Act
            HttpResponseMessage response = await rb.SendAsync(method)
                .ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should()
                .BeTrue($"'{method}' HTTP method must be supported");
            ((int)response.StatusCode).Should().Be(Status200OK);

        }

        [Fact]
        public async Task Create_Resource()
        {
            // Arrange
            PatientInfo resourceToCreate = new PatientInfo
            {
                Firstname = "victor",
                Lastname = "zsasz"
            };

            JSchema createdResourceSchema = new JSchema
            {
                Type = JSchemaType.Object,
                Properties =
                {
                    [nameof(BrowsableResource<PatientInfo>.Resource).ToLower()] = new JSchema
                    {
                        Type = JSchemaType.Object,
                        Properties =
                        {
                            [nameof(PatientInfo.Firstname).ToLower()] = new JSchema { Type = JSchemaType.String },
                            [nameof(PatientInfo.Lastname).ToLower()] = new JSchema { Type = JSchemaType.String },
                            [nameof(PatientInfo.Fullname).ToLower()] = new JSchema { Type = JSchemaType.String },
                            [nameof(PatientInfo.UpdatedDate).ToLower()] = new JSchema { Type = JSchemaType.String },
                            [nameof(PatientInfo.Id).ToLower()] = new JSchema { Type = JSchemaType.String },
                            [nameof(PatientInfo.BirthDate).ToLower()] = new JSchema { Type = JSchemaType.String },
                        },
                        AllowAdditionalItems = false
                    }
                },
                AllowAdditionalItems = false
            };

            RequestBuilder requestBuilder = new RequestBuilder(_server, "/measures/patients")
                .AddHeader("Accept", "application/json");

            // Act
            HttpResponseMessage response = await requestBuilder.PostAsync()
                .ConfigureAwait(false);

            // Assert
            ((int)response.StatusCode).Should().Be(StatusCodes.Status201Created, $"The resource was created");

            string json = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            JToken jToken = JToken.Parse(json);
            jToken.IsValid(createdResourceSchema).Should()
                .BeTrue();

            BrowsableResource<PatientInfo> browsableResource = jToken.ToObject<BrowsableResource<PatientInfo>>();

            Guid id = browsableResource.Resource.Id;
            id.Should()
                .NotBeEmpty();

            RequestBuilder rb = new RequestBuilder(_server, $"/measures/patients/{id}")
                .And((request) => request.Method = HttpMethod.Head);

            HttpResponseMessage checkResponse = await rb.GetAsync()
                .ConfigureAwait(false);

            ((int)checkResponse.StatusCode).Should()
                .Be(StatusCodes.Status204NoContent, $"The resource <{id}> previously created must exist on the server after being created");
        }
    }
}
