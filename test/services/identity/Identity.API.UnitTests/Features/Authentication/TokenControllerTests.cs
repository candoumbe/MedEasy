using FluentAssertions;
using Identity.API.Features.Authentication;
using Identity.CQRS.Commands;
using Identity.CQRS.Queries.Accounts;
using Identity.DataStores.SqlServer;
using Identity.DTO;
using MedEasy.DAL.Context;
using MedEasy.DAL.Interfaces;
using MedEasy.Identity.API.Features.Authentication;
using MedEasy.IntegrationTests.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Optional;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Moq.MockBehavior;

namespace Identity.API.UnitTests.Features.Authentication
{
    [UnitTest]
    [Feature("Identity")]
    [Feature("Accounts")]
    public class TokenControllerUnitTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IMediator> _mediatorMock;
        private Mock<IOptionsSnapshot<JwtOptions>> _jwtOptionsMock;
        private TokenController _controller;
        private IUnitOfWorkFactory _unitOfWorkFactory;

        public TokenControllerUnitTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture databaseFixture)
        {
            _outputHelper = outputHelper;
            _mediatorMock = new Mock<IMediator>(Strict);

            DbContextOptionsBuilder<IdentityContext> optionsBuilder = new DbContextOptionsBuilder<IdentityContext>();
            optionsBuilder.UseSqlite(databaseFixture.Connection);

            _unitOfWorkFactory = new EFUnitOfWorkFactory<IdentityContext>(optionsBuilder.Options, (options) =>
            {
                IdentityContext context = new IdentityContext(options);
                context.Database.EnsureCreated();

                return context;

            });

            _jwtOptionsMock = new Mock<IOptionsSnapshot<JwtOptions>>(Strict);
            _jwtOptionsMock.Setup(mock => mock.Value)
                .Returns(new JwtOptions { Issuer = "identity.api", Audiences = new[] { "identity.api", "measures.api" }, Validity = 10, Key = "key_to_secure_api_access" });

            _controller = new TokenController(mediator: _mediatorMock.Object, jwtOptions: _jwtOptionsMock.Object);
        }

        public void Dispose()
        {
            _outputHelper = null;
            _mediatorMock = null;
            _controller = null;
            _unitOfWorkFactory = null;
            _jwtOptionsMock = null;

        }

        [Fact]
        public async Task GivenNoAccount_Post_Returns_Unauthorized()
        {
            // Arrange
            LoginModel model = new LoginModel { Username = "Bruce", Password = "CapedCrusader" };
            _mediatorMock.Setup(mock => mock.Send(It.IsNotNull<GetOneAccountByUsernameAndPasswordQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.None<AccountInfo>());


            // Act
            IActionResult actionResult = await _controller.Post(model, ct : default(CancellationToken))
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsNotNull<GetOneAccountByUsernameAndPasswordQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetOneAccountByUsernameAndPasswordQuery>(q => q.Data.Username == model.Username && q.Data.Password == model.Password), It.IsAny<CancellationToken>()), Times.Once);
            _jwtOptionsMock.Verify(mock => mock.Value, Times.Never);

            actionResult.Should()
                .BeAssignableTo<UnauthorizedResult>();

        }


        [Fact]
        public async Task GivenAccountExists_Post_Returns_ValidToken()
        {
            // Arrange
            LoginModel model = new LoginModel { Username = "Bruce", Password = "CapedCrusader" };
            AccountInfo accountInfo = new AccountInfo
            {
                Id = Guid.NewGuid(),
                Username = model.Username,
                Email = "brucewayne@gotham.com",
                Firstname = "Bruce",
                Lastname = "Wayne"
            };
            _mediatorMock.Setup(mock => mock.Send(It.IsNotNull<GetOneAccountByUsernameAndPasswordQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.Some(accountInfo));
            _mediatorMock.Setup(mock => mock.Send(It.IsNotNull<CreateAuthenticationTokenCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new JwtSecurityToken());

            // Act
            IActionResult actionResult = await _controller.Post(model, ct: default(CancellationToken))
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsNotNull<GetOneAccountByUsernameAndPasswordQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetOneAccountByUsernameAndPasswordQuery>(q => q.Data.Username == model.Username && q.Data.Password == model.Password), It.IsAny<CancellationToken>()), Times.Once);

            _mediatorMock.Verify(mock => mock.Send(It.IsNotNull<CreateAuthenticationTokenCommand>(), It.IsAny<CancellationToken>()));

            _jwtOptionsMock.Verify(mock => mock.Value, Times.Once);

            actionResult.Should()
                .BeAssignableTo<OkObjectResult>().Which
                .Value.Should()
                    .BeAssignableTo<SecurityToken>();

        }
    }
}
