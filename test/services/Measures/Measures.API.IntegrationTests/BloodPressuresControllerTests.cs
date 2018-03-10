using FluentAssertions;
using FluentAssertions.Extensions;
using Measures.API.Features.Patients;
using Measures.Context;
using Measures.DTO;
using MedEasy.DAL.Context;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
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
    [Feature("Blood pressures")]
    [Feature("Measures")]
    public class BloodPressuresControllerTests : IDisposable, IClassFixture<ServicesTestFixture<Startup>>
    {
        private TestServer _server;
        private ITestOutputHelper _outputHelper;
        private const string _endpointUrl = "/measures/bloodpressures";
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
        public async Task GetAll_With_No_Data()
        {
            // Arrange
            RequestBuilder rb = _server.CreateRequest(_endpointUrl)
                .AddHeader("Accept", "application/json")
                .AddHeader("Accept-Charset", "utf-8");

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
        [MemberData(nameof(GetAll_With_Invalid_Pagination_Returns_BadRequestCases))]
        public async Task GetAll_With_Invalid_Pagination_Returns_BadRequest(int page, int pageSize)
        {

            _outputHelper.WriteLine($"Paging configuration : {SerializeObject(new { page, pageSize })}");

            // Arrange
            RequestBuilder rb = _server.CreateRequest($"{_endpointUrl}?page={page}&pageSize={pageSize}")
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
                .NotBeEmpty();


        }


        [Theory]
        [InlineData(_endpointUrl, "GET")]
        [InlineData(_endpointUrl, "HEAD")]
        [InlineData(_endpointUrl, "OPTIONS")]
        public async Task ShouldReturnsSuccessCode(string url, string method)
        {

            _outputHelper.WriteLine($"URL : <{url}>");
            _outputHelper.WriteLine($"method : <{method}>");

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
            RequestBuilder requestBuilder = new RequestBuilder(_server, url)
                .AddHeader("Accept", "application/json");

            // Act
            HttpResponseMessage response = await requestBuilder.SendAsync(method)
                .ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should()
                .BeFalse("the requested bloodpressure id is empty");
            ((int)response.StatusCode).Should()
                .Be(Status400BadRequest, "the requested bloodpressure id is empty");

            ((int)response.StatusCode).Should().Be(Status400BadRequest, "the requested bloodpressure id is not empty and it's part of the url");

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
        public async Task Patch_With_EmptyId_Returns_Bad_Request()
        {
            // Arrange
            JsonPatchDocument<BloodPressureInfo> changes = new JsonPatchDocument<BloodPressureInfo>();
            changes.Replace(x => x.SystolicPressure, 120);
            
            RequestBuilder requestBuilder = new RequestBuilder(_server, $"{_endpointUrl}/{Guid.Empty}")
                .AddHeader("Accept", "application/json")
                .And(request => request.Content = new StringContent(changes.ToString(), Encoding.UTF8, "application/json-patch+json"));

            // Act
            HttpResponseMessage response = await requestBuilder.SendAsync(Patch)
                .ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should()
                .BeFalse();
            ((int)response.StatusCode).Should()
                .Be(Status400BadRequest);

            string json = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"Bad request content : {json}");

            json.Should()
                .NotBeNullOrEmpty("A body describing the error must be provided");

            JToken errorToken = JToken.Parse(json);

            errorToken.IsValid(_errorObjectSchema).Should()
                .BeTrue();

            ErrorObject errorObject = errorToken.ToObject<ErrorObject>();
            errorObject.Code.Should()
                .Be("BAD_REQUEST");
            errorObject.Description.Should()
                .Be("Validation failed");
            errorObject.Errors.Should()
                .HaveCount(1).And
                .ContainKey("id").WhichValue.Should()
                    .HaveCount(1).And
                    .HaveElementAt(0, "'id' must have a non default value");





        }

        [Theory]
        [InlineData("HEAD", "")]
        [InlineData("GET", "")]
        public async Task GivenInvalidQueryString_CallingSearch_ShouldReturns_BadRequest(string method, string queryString)
        {
            // Arrange
            string url = $"{_endpointUrl}/search{queryString}";

            _outputHelper.WriteLine($"URL : {url}");
            _outputHelper.WriteLine($"Method : {method}");

            RequestBuilder requestBuilder = new RequestBuilder(_server, url);

            // Act
            HttpResponseMessage response = await requestBuilder.SendAsync(method)
                .ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should().BeFalse($"a non empty query string must be provided to the <{url}>");
            ((int)response.StatusCode).Should().Be(Status400BadRequest);
        }

    }
}
