using FluentAssertions;
using FluentAssertions.Extensions;
using Identity.API.Features.Auth;
using Identity.API.Features.Auth.v2;
using Identity.CQRS.Commands;
using Identity.CQRS.Queries.Accounts;
using Identity.DataStores.SqlServer;
using Identity.DTO;
using Identity.DTO.v2;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Optional;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Moq.MockBehavior;

namespace Identity.API.UnitTests.Features.v2.Auth
{
    [UnitTest]
    [Feature("Identity")]
    [Feature("Accounts")]
    public class TokenControllerUnitTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IMediator> _mediatorMock;
        private Mock<IOptionsSnapshot<JwtOptions>> _jwtOptionsMock;
        private JwtOptions _jwtOptions;
        private Mock<IHttpContextAccessor> _httpContextMock;
        private TokenController _sut;
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
            _jwtOptions = new JwtOptions
            {
                Issuer = "identity.api",
                Audiences = new[] {
                    "identity.api",
                    "measures.api"
                },
                AccessTokenLifetime = 10,
                RefreshTokenLifetime = 20,
                Key = "key_to_secure_api_access"
            };
            _jwtOptionsMock.Setup(mock => mock.Value)
                .Returns(_jwtOptions);
            _httpContextMock = new Mock<IHttpContextAccessor>(Strict);
            _sut = new TokenController(mediator: _mediatorMock.Object, jwtOptions: _jwtOptionsMock.Object, _httpContextMock.Object);
        }

        public void Dispose()
        {
            _outputHelper = null;
            _mediatorMock = null;
            _sut = null;
            _unitOfWorkFactory = null;
            _jwtOptionsMock = null;
            _httpContextMock = null;
            _jwtOptions = null;
        }

        [Fact]
        public async Task GivenAccountDoesNotExist_Post_Returns_NotFound()
        {
            // Arrange
            LoginModel model = new LoginModel { Username = "Bruce", Password = "CapedCrusader" };
            _mediatorMock.Setup(mock => mock.Send(It.IsNotNull<GetOneAccountByUsernameAndPasswordQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.None<AccountInfo>());

            // Act
            ActionResult<BearerTokenInfo> actionResult = await _sut.Post(model, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsNotNull<GetOneAccountByUsernameAndPasswordQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetOneAccountByUsernameAndPasswordQuery>(q => q.Data.Username == model.Username && q.Data.Password == model.Password), It.IsAny<CancellationToken>()), Times.Once);
            _jwtOptionsMock.Verify(mock => mock.Value, Times.Never);

            actionResult.Should()
                .NotBeNull();
            actionResult.Result.Should()
                .BeAssignableTo<NotFoundResult>("The account with the specified credentials was not found");
        }

        [Fact]
        public async Task GivenAccountExists_Post_Returns_ValidToken()
        {
            // Arrange
            LoginModel model = new LoginModel { Username = "Bruce", Password = "CapedCrusader" };
            AuthenticationInfo authenticationInfo = new AuthenticationInfo { Location = "Paris" };
            DateTime accessTokenExpiresDate = 10.January(2010).Add(12.Hours())
                .ToUniversalTime();
            DateTime refreshTokenExpiresDate = 10.January(2010).Add(23.Hours().And(59.Minutes().And(59.Seconds())))
                .ToUniversalTime();
            AccountInfo accountInfo = new AccountInfo
            {
                Id = Guid.NewGuid(),
                Username = model.Username,
                Email = "brucewayne@gotham.com",
                Name = "Bruce Wayne"
            };
            _httpContextMock.Setup(mock => mock.HttpContext.Request.Headers)
                .Returns(new HeaderDictionary(new Dictionary<string, StringValues>
                {
                    ["X_FORWARDED_FOR"] = new StringValues(authenticationInfo.Location)
                }));
            _mediatorMock.Setup(mock => mock.Send(It.IsNotNull<GetOneAccountByUsernameAndPasswordQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.Some(accountInfo));
            _mediatorMock.Setup(mock => mock.Send(It.IsNotNull<CreateAuthenticationTokenCommand>(), It.IsAny<CancellationToken>()))
                .Returns((CreateAuthenticationTokenCommand cmd, CancellationToken ct) =>
                    {
                        (AuthenticationInfo authInfo, AccountInfo localAccountInfo, JwtInfos jwtInfos) = cmd.Data;

                        return Task.FromResult(new AuthenticationTokenInfo
                        {
                            AccessToken = new JwtSecurityToken(
                                issuer: _jwtOptions.Issuer,
                                expires: accessTokenExpiresDate,
                                claims: jwtInfos.Audiences.Select(aud => new Claim(JwtRegisteredClaimNames.Aud, aud))
                            ),
                            RefreshToken = new JwtSecurityToken(
                                issuer: _jwtOptions.Issuer,
                                expires: refreshTokenExpiresDate,
                                claims: jwtInfos.Audiences
                                    .Select(aud => new Claim(JwtRegisteredClaimNames.Aud, aud))
                                    .Concat(new[] { new Claim(CustomClaimTypes.Location, authInfo.Location) })
                            ),
                        });
                    }
                  );

            // Act
            ActionResult<BearerTokenInfo> actionResult = await _sut.Post(model, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsNotNull<GetOneAccountByUsernameAndPasswordQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetOneAccountByUsernameAndPasswordQuery>(q => q.Data.Username == model.Username && q.Data.Password == model.Password), It.IsAny<CancellationToken>()), Times.Once);

            _mediatorMock.Verify(mock => mock.Send(It.IsNotNull<CreateAuthenticationTokenCommand>(), It.IsAny<CancellationToken>()));
            _mediatorMock.Verify(mock => mock.Send(It.Is<CreateAuthenticationTokenCommand>(cmd => cmd.Data.authInfo.Location == authenticationInfo.Location
                && cmd.Data.jwtInfos.Issuer == _jwtOptions.Issuer
                && cmd.Data.jwtInfos.Key == _jwtOptions.Key
                && cmd.Data.jwtInfos.Audiences.All(audience => _jwtOptions.Audiences.Contains(audience))
                && cmd.Data.jwtInfos.AccessTokenLifetime == _jwtOptions.AccessTokenLifetime
                && cmd.Data.jwtInfos.RefreshTokenLifetime == _jwtOptions.RefreshTokenLifetime), It.IsAny<CancellationToken>()));

            _jwtOptionsMock.Verify(mock => mock.Value, Times.Once);

            actionResult.Should()
                .NotBeNull();
            actionResult.Value.Should()
                .NotBeNull();

            BearerTokenInfo bearerToken = actionResult.Value;

            TokenInfo accessTokenInfo = bearerToken.AccessToken;
            accessTokenInfo.Should()
                .NotBeNull();
            accessTokenInfo.Token.Should()
                .NotBeNullOrWhiteSpace();
            accessTokenInfo.Expires.Should()
                .Be(accessTokenExpiresDate);

            TokenInfo refreshTokenInfo = bearerToken.RefreshToken;
            refreshTokenInfo.Should()
                .NotBeNull();
            refreshTokenInfo.Token.Should()
                .NotBeNullOrWhiteSpace();
            refreshTokenInfo.Expires.Should()
                .Be(refreshTokenExpiresDate);
        }
    }
}
