using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using FluentAssertions;
using MedEasy.DTO;
using Microsoft.AspNetCore.JsonPatch;
using static Newtonsoft.Json.JsonConvert;
using System.IO;
using System.Text;

namespace MedEasy.API.IntegrationTests
{
    public class PatientsControllerTests : IDisposable
    {
        private HttpClient _client;
        private TestServer _server;

        public PatientsControllerTests()
        {
            IWebHostBuilder webHostBuilder = new WebHostBuilder()
                .UseStartup<Startup>()
                .UseEnvironment("Development");

            _server = new TestServer(webHostBuilder);

            _client = _server.CreateClient();
        }

        public void Dispose()
        {
            _client = null;
            _server = null;
        }


        [Fact]
        [Trait("Category", "Integration")]
        public async Task Patching_Resource_Id_Should_Returns_BadRequest()
        {
            // Arrange
            JsonPatchDocument<PatientInfo> patchDocument = new JsonPatchDocument<PatientInfo>();

            // Act
            HttpResponseMessage response = await new RequestBuilder(_server, $"/{Guid.NewGuid()}")
                .AddHeader("Accept-Type", "application/json")
                .And((request) => {
                    request.Content = new StringContent(SerializeObject(patchDocument.Operations));
                })
                .SendAsync("PATCH");

            // Assert
            response.IsSuccessStatusCode.Should().BeFalse();
            response.Content.Should()
                .NotBeNull();

            using (var ms = new MemoryStream())
            {
                await response.Content.CopyToAsync(ms);
                string json = Encoding.UTF8.GetString(ms.ToArray());

                
            }

           
        }
    }
}
