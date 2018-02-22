using FluentAssertions;
using MedEasy.IntegrationTests.Core;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Microsoft.AspNetCore.Http.HttpMethods;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Measures.API.IntegrationTests
{
    [IntegrationTest]
    public class RootControllerTests : IDisposable, IClassFixture<ServicesTestFixture<Startup>>
    {
        private TestServer _server;
        private ITestOutputHelper _outputHelper;
        private const string _baseAddress = "https://url/to/services";


        public RootControllerTests(ITestOutputHelper outputHelper, ServicesTestFixture<Startup> fixture)
        {
            _outputHelper = outputHelper;
            fixture.Initialize(Path.Combine("..", "..", "..", "..", "src", "services", "Measures"), "Development", "Measures.API");
            _server = fixture.Server;
            _server.BaseAddress = new Uri(_baseAddress);

        }


        public void Dispose()
        {
            _outputHelper = null;
            _server.Dispose();

        }

        [Theory]
        [InlineData("/")]
        [InlineData("/measures")]
        public async Task Calling_ApiIndex_WithJson_Returns_Root_Informations(string url)
        {
            // Arrange
            JSchema endpointSchema = new JSchema
            {
                Type = JSchemaType.Object,
                Properties =
                {
                    [nameof(Endpoint.Name)] = new JSchema { Type = JSchemaType.String, MinimumLength = 1 },
                    [nameof(Endpoint.Forms)] = new JSchema { Type = JSchemaType.Array },
                    [nameof(Endpoint.Link)] = new JSchema
                    {
                        Type = JSchemaType.Object,
                        Properties =
                        {
                            [nameof(Link.Href)] = new JSchema { Type = JSchemaType.String, MinimumLength = 1},
                            [nameof(Link.Relation)] = new JSchema { Type = JSchemaType.String, MinimumLength = 1},
                            [nameof(Link.Method)] = new JSchema
                            {
                                Type = JSchemaType.String,
                                MinimumLength = 1,
                                Enum =
                                {
                                    JToken.FromObject(Get),
                                    JToken.FromObject(Head),
                                    JToken.FromObject(Post),
                                    JToken.FromObject(Put),
                                    JToken.FromObject(Patch),
                                    JToken.FromObject(Delete),
                                }
                            }
                        },
                        Required = { nameof(Link.Href) , nameof(Link.Relation), nameof(Link.Method) }
                    }
                }
            };

            JSchema rootResponseSchema = new JSchema
            {
                Type = JSchemaType.Array,
                MinimumItems = 1,
                UniqueItems = true
            };


            RequestBuilder rb = _server.CreateRequest(url)
                .AddHeader("Accept", "application/json")
                .AddHeader("Accept-Charset", "utf-8");

            // Act
            HttpResponseMessage response = await rb.GetAsync()
                .ConfigureAwait(false);

            // Assert
            ((int)response.StatusCode).Should().Be(Status200OK);
            HttpContentHeaders headers = response.Content.Headers;

            headers.ContentType.MediaType.Should().BeEquivalentTo("application/json");

            string json = await response.Content.ReadAsStringAsync();

            _outputHelper.WriteLine($"json : {json}");

            JToken.Parse(json).IsValid(rootResponseSchema).Should()
                .BeTrue();

            JArray array = JArray.Parse(json);
            array.Should()
                .OnlyContain(item => item.IsValid(endpointSchema));
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/measures")]
        public async Task Calling_ApiIndex_WithXml_Returns_Root_Informations(string url)
        {
            // Arrange
            RequestBuilder rb = _server.CreateRequest(url)
                .AddHeader("Accept", "application/xml")
                .AddHeader("Accept-Charset", "uf-8");

            // Act
            HttpResponseMessage response = await rb.GetAsync()
                .ConfigureAwait(false);

            // Assert
            ((int)response.StatusCode).Should().Be(Status200OK);
            HttpContentHeaders headers = response.Content.Headers;
            headers.ContentType.MediaType.Should().BeEquivalentTo("application/xml");
        }


        [Theory]
        [InlineData("/", "GET")]
        [InlineData("/", "OPTIONS")]
        [InlineData("/measures", "GET")]
        [InlineData("/measures", "OPTIONS")]
        public async Task ShouldReturnsSuccessCode(string url, string method)
        {

            // Arrange
            RequestBuilder rb = _server.CreateRequest(url)
                .And(request => request.Method = new HttpMethod(method));


            // Act
            HttpResponseMessage response = await rb.GetAsync()
                .ConfigureAwait(false);

            // Assert
            ((int)response.StatusCode).Should().Be(Status200OK);




        }


        [Theory]
        [InlineData("/", "GET")]
        [InlineData("/", "OPTIONS")]
        [InlineData("/measures", "GET")]
        [InlineData("/measures", "OPTIONS")]
        public async Task ShouldReturnsSuccessCode_When_Posting_With_Method_Overrides(string url, string methodOverride)
        {
            _outputHelper.WriteLine($"Url : <{url}>");
            _outputHelper.WriteLine($"Override Method : <{methodOverride}>");

            // Arrange
            RequestBuilder rb = _server.CreateRequest(url)
                .AddHeader("x-http-method-override", methodOverride);


            // Act
            HttpResponseMessage response = await rb.PostAsync()
                .ConfigureAwait(false);

            // Assert
            ((int)response.StatusCode).Should().Be(Status200OK);
        }

    }
}
