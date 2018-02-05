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
using static Microsoft.AspNetCore.Http.HttpMethods;
using static Newtonsoft.Json.JsonConvert;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Measures.API.IntegrationTests
{
    [Collection("IntegrationTests")]
    public class BloodPressuresControllerTests : IDisposable, IClassFixture<ServicesTestFixture<Startup>>
    {
        private TestServer _server;
        private ITestOutputHelper _outputHelper;
        private const string _endpointUrl = "/measures/bloodpressures";

        public BloodPressuresControllerTests(ITestOutputHelper outputHelper, ServicesTestFixture<Startup> fixture)
        {
            _outputHelper = outputHelper;
            fixture.Initialize(
                relativeTargetProjectParentDir: Path.Combine("..", "..", "..", "..", "src", "services", "Measures"),
                environmentName: "IntegrationTest",
                applicationName: "Measures.API",
                initializeServices: (services) =>
                    services.AddSingleton<IUnitOfWorkFactory, EFUnitOfWorkFactory<MeasuresContext>>(provider =>
                    {
                        DbContextOptionsBuilder<MeasuresContext> builder = new DbContextOptionsBuilder<MeasuresContext>();
                        builder.UseInMemoryDatabase($"InMemoryDb_{Guid.NewGuid()}");

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
        [Trait("Category", "Integration")]
        [Trait("Resource", "BloodPressures")]
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

            RequestBuilder rb = _server.CreateRequest(_endpointUrl)
                .AddHeader("Accept", "application/json")
                .AddHeader("Accept-Charset", "uf-8");

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
            jToken.IsValid(pageResponseSchema).Should().BeTrue();
        }


        public static IEnumerable<object[]> GetAll_With_Invalid_Pagination_Returns_BadRequestCases
        {
            get
            {
                int[] invalidPages = { int.MinValue, -1, -10, 0 };

                IEnumerable<(int page, int pageSize)> invalidCases = invalidPages.CrossJoin(invalidPages)
                    .Where(tuple => tuple.Item1 <= 0 || tuple.Item2 <= 0);

                foreach (var (page, pageSize) in invalidCases)
                {
                    yield return new object[] { page, pageSize };
                }
            }
        }

        [Theory]
        [Trait("Category", "Integration")]
        [Trait("Resource", "BloodPressures")]
        [MemberData(nameof(GetAll_With_Invalid_Pagination_Returns_BadRequestCases))]
        public async Task GetAll_With_Invalid_Pagination_Returns_BadRequest(int page, int pageSize)
        {
            // Arrange
            RequestBuilder rb = _server.CreateRequest($"{_endpointUrl}?page={page}&pageSize={pageSize}")
                .AddHeader("Accept", "application/json");

            // Act
            HttpResponseMessage response = await rb.GetAsync()
                .ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should().BeFalse("Invalid page and/or pageSize");
            ((int)response.StatusCode).Should().Be(Status406NotAcceptable);

        }


        [Theory]
        [InlineData(_endpointUrl, "GET")]
        [InlineData(_endpointUrl, "HEAD")]
        [InlineData(_endpointUrl, "OPTIONS")]
        [Trait("Category", "Integration")]
        [Trait("Resource", "BloodPressures")]
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
        [Trait("Category", "Integration")]
        [Trait("Resource", "BloodPressures")]
        public async Task Create_Resource()
        {
            // Arrange
            CreateBloodPressureInfo resourceToCreate = new CreateBloodPressureInfo
            {
                SystolicPressure = 120,
                DiastolicPressure = 80,
                DateOfMeasure = 23.January(2002).AddHours(23).AddMinutes(36),
                Patient = new PatientInfo
                {
                    Firstname = "victor",
                    Lastname = "zsasz"
                }
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

            RequestBuilder requestBuilder = new RequestBuilder(_server, _endpointUrl)
                .AddHeader("Content-Type", "application/json")
                .And((request) =>
                    request.Content = new StringContent(SerializeObject(resourceToCreate), Encoding.UTF8, "application/json")
                );

            // Act
            HttpResponseMessage response = await requestBuilder.PostAsync()
                .ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue($"Creating a valid {nameof(BloodPressureInfo)} resource must succeed");
            ((int)response.StatusCode).Should().Be(Status201Created, $"The resource was created");

            string json = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            JToken jToken = JToken.Parse(json);
            jToken.IsValid(createdResourceSchema).Should()
                .BeTrue();

            Uri location = response.Headers.Location;
            _outputHelper.WriteLine($"Location of the resource : <{location}>");
            location.Should().NotBeNull();
            location.IsAbsoluteUri.Should().BeTrue("location of the resource must be an absolute URI");

            requestBuilder = new RequestBuilder(_server, response.Headers.Location.ToString());

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
                    $"No {nameof(CreateBloodPressureInfo.Patient)} data set onto the resource"
                };
            }
        }

        [Theory]
        [MemberData(nameof(InvalidRequestToCreateABloodPressureResourceCases))]
        [Trait("Resource", "BloodPressures")]
        public async Task PostInvalidResource_Returns_BadRequest(CreateBloodPressureInfo invalidResource, string reason)
        {

            // Arrange
            RequestBuilder requestBuilder = new RequestBuilder(_server, _endpointUrl)
                .And(request => request.Content = new StringContent(SerializeObject(invalidResource), Encoding.UTF8, "application/json"));

            // Act
            HttpResponseMessage response = await requestBuilder.PostAsync()
                .ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should().BeFalse(reason);
            ((int)response.StatusCode).Should().Be(Status406NotAcceptable, reason);
            response.ReasonPhrase.Should().NotBeNullOrWhiteSpace();


        }

        
        

    }
}
