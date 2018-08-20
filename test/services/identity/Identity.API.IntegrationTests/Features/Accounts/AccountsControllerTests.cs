using Identity.DataStores.SqlServer;
using Identity.DTO;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static System.Text.Encoding;
using static Newtonsoft.Json.JsonConvert;
using FluentAssertions;
using static Microsoft.AspNetCore.Http.StatusCodes;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using MedEasy.RestObjects;
using System.Collections.Generic;
using System.Linq;
using Identity.API.Fixtures;
using Microsoft.AspNetCore.Http;

namespace Identity.API.IntegrationTests.Features.Accounts
{
    [IntegrationTest]
    [Feature("Accounts")]
    public class AccountsControllerTests : IDisposable, IClassFixture<ServicesTestFixture<Startup>>
    {
        private TestServer _server;
        private ITestOutputHelper _outputHelper;
        private const string _endpointUrl = "/identity";
        
        
        
        public AccountsControllerTests(ITestOutputHelper outputHelper, ServicesTestFixture<Startup> fixture)
        {
            _outputHelper = outputHelper;
            fixture.Initialize(
                relativeTargetProjectParentDir: Path.Combine("..", "..", "..", "..", "src", "services", "Identity"),
                environmentName: "IntegrationTest",
                applicationName: typeof(Startup).Assembly.GetName().Name);
            

            _server = fixture.Server;
        }


        public void Dispose()
        {
            _outputHelper = null;
            _server?.Dispose();
            _server = null;
        }

       

        [Fact]
        public async Task GivenAccount_Post_Creates_Record()
        {
            // Arrange
            NewAccountInfo newAccount = new NewAccountInfo
            {
                Name = "Cyrille NDOUMBE",
                Username = "candoumbe",
                Password = "candoumbe",
                ConfirmPassword = "candoumbe",
                Email = "candoumbe@medeasy.fr"
            };

            RequestBuilder request = _server.CreateRequest($"{_endpointUrl}/accounts")
                .And(msg => msg.Content = new StringContent(SerializeObject(newAccount), UTF8,"application/json"));
            // Act
            HttpResponseMessage response = await request.PostAsync()
                .ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(Status201Created);
            
            string jsonResponse = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            JToken jsonToken = JToken.Parse(jsonResponse);
            JSchema responseSchema = new JSchemaGenerator()
                .Generate(typeof(BrowsableResource<AccountInfo>));
            jsonToken.IsValid(responseSchema);

            BrowsableResource<AccountInfo> browsableResource = jsonToken.ToObject<BrowsableResource<AccountInfo>>();

            IEnumerable<Link> links = browsableResource.Links;
            links.Should()
                .NotBeEmpty().And
                .NotContainNulls().And
                .NotContain(link => string.IsNullOrWhiteSpace(link.Href), $"{nameof(Link.Href)} must be explicitely specified for each link").And
                .NotContain(link => string.IsNullOrWhiteSpace(link.Relation), $"{nameof(Link.Relation)} must be explicitely specified for each link").And
                .NotContain(link => string.IsNullOrWhiteSpace(link.Method), $"{nameof(Link.Method)} must be explicitely specified for each link").And
                .Contain(link => link.Relation == LinkRelation.Self, $"a direct link to the resource must be provided").And
                .HaveCount(1);

            Link linkToSelf = links.Single(link => link.Relation == LinkRelation.Self);
            linkToSelf.Method.Should()
                .Be("GET");


            AccountInfo createdAccountInfo = browsableResource.Resource;

            createdAccountInfo.Name.Should()
                .Be(newAccount.Name);
            createdAccountInfo.Email.Should()
                .Be(newAccount.Email);

            createdAccountInfo.Id.Should()
                .NotBeEmpty();
        }

        [Fact]
        public async Task GivenValidToken_Get_Returns_ListOfAccounts()
        {
            // Arrange
            NewAccountInfo newAccount = new NewAccountInfo
            {
                Name = "Cyrille NDOUMBE",
                Username = "candoumbe",
                Password = "candoumbe",
                ConfirmPassword = "candoumbe",
                Email = "candoumbe@medeasy.fr"
            };

            BearerTokenInfo bearerInfo = await IdentityApiFixture.Register(_server, newAccount)
                .ConfigureAwait(false);

            RequestBuilder requestBuilder = new RequestBuilder(_server, $"{_endpointUrl}/accounts")
                .AddHeader("Authorization", $"Bearer {bearerInfo.AccessToken}");

            // Act
            HttpResponseMessage response = await requestBuilder.GetAsync()
                .ConfigureAwait(false);

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(Status200OK);

            string jsonResponse = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            JToken jsonToken = JToken.Parse(jsonResponse);
            JSchema responseSchema = new JSchemaGenerator()
                .Generate(typeof(GenericPagedGetResponse<BrowsableResource<AccountInfo>>));
            jsonToken.IsValid(responseSchema);
        }


    }
}
