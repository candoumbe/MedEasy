using FluentAssertions;
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
using static Newtonsoft.Json.JsonConvert;
using static System.Text.Encoding;
using static Microsoft.AspNetCore.Http.StatusCodes;
using Newtonsoft.Json.Linq;

namespace Identity.API.IntegrationTests.Features.Auth
{
    [IntegrationTest]
    [Feature("Authentication")]
    public class TokenControllerTests : IDisposable, IClassFixture<ServicesTestFixture<Startup>>, IClassFixture<SqliteDatabaseFixture>
    {
        private ITestOutputHelper _outputHelper;
        private TestServer _server;
        private readonly string _endpointUrl = "/identity";

        public TokenControllerTests(ITestOutputHelper outputHelper, ServicesTestFixture<Startup> identityFixture, SqliteDatabaseFixture sqliteDatabaseFixture)
        {
            _outputHelper = outputHelper;
            identityFixture.Initialize(
                relativeTargetProjectParentDir: Path.Combine("..", "..", "..", "..", "src", "services", "Identity"),
                environmentName: "IntegrationTest",
                applicationName: typeof(Startup).Assembly.GetName().Name,
                overrideServices: (services) =>
                    services.AddSingleton<IUnitOfWorkFactory, EFUnitOfWorkFactory<IdentityContext>>(provider =>
                    {
                        DbContextOptionsBuilder<IdentityContext> builder = new DbContextOptionsBuilder<IdentityContext>();
                        builder.UseSqlite(sqliteDatabaseFixture.Connection);

                        return new EFUnitOfWorkFactory<IdentityContext>(builder.Options, (options) => {

                            IdentityContext context = new IdentityContext(options);
                            context.Database.EnsureCreated();
                            return context;
                        });

                    })
                );


            _server = identityFixture.Server;
        }

        public void Dispose()
        {
            _outputHelper = null;
            _server = null;
        }

        [Fact]
        public async Task GivenUserExists_Token_ReturnsValidToken()
        {
            // Arrange
            const string password = "thecapedcrusader";
            NewAccountInfo newAccountInfo = new NewAccountInfo
            {
                Name = "Bruce Wayne",
                Username = "thebatman",
                Password = password,
                ConfirmPassword = password,
                Email = "bruce.wayne@gotham.com"
            };

            RequestBuilder request = _server.CreateRequest($"{_endpointUrl}/accounts")
                .And(msg => msg.Content = new StringContent(SerializeObject(newAccountInfo), UTF8, "application/json"));

            await request.PostAsync()
                .ConfigureAwait(false);


            LoginInfo loginInfo = new LoginInfo
            {
                Username = newAccountInfo.Username,
                Password = newAccountInfo.Password
            };
            
            request = _server.CreateRequest("/auth/token")
                .And(msg => msg.Content = new StringContent(SerializeObject(loginInfo), UTF8, "application/json"));

            // Act
            HttpResponseMessage response = await request.PostAsync()
                .ConfigureAwait(false);

            // Assert
            _outputHelper.WriteLine($"Status code : {response.StatusCode}");

            response.IsSuccessStatusCode.Should()
                .BeTrue();
            response.StatusCode.Should()
                .Be(Status200OK);

            string json = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"HTTP content : '{json}'");

            BearerTokenInfo tokenInfo = JToken.Parse(json)
                .ToObject<BearerTokenInfo>();

            tokenInfo.Token.Should()
                .NotBeNullOrWhiteSpace();


        }

    }
}
