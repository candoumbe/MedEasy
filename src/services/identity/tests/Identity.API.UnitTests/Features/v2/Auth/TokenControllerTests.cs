namespace Identity.API.UnitTests.Features.v2.Auth
{
    using FluentAssertions;
    using FluentAssertions.Extensions;

    using Identity.API.Features.Auth;
    using Identity.API.Features.v1.Auth;
    using Identity.CQRS.Commands;
    using Identity.CQRS.Queries.Accounts;
    using Identity.DataStores;
    using Identity.DTO;
    using Identity.DTO.v2;
    using Identity.Ids;

    using MedEasy.DAL.EFStore;
    using MedEasy.DAL.Interfaces;
    using MedEasy.IntegrationTests.Core;
    using MedEasy.ValueObjects;

    using MediatR;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;

    using Moq;

    using NodaTime;
    using NodaTime.Testing;

    using Optional;

    using System;
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

    [UnitTest]
    [Feature("Identity")]
    [Feature("Accounts")]
    public class TokenControllerUnitTests : IClassFixture<SqliteEfCoreDatabaseFixture<IdentityDataStore>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IOptionsMonitor<JwtOptions>> _jwtOptionsMock;
        private readonly JwtOptions _jwtOptions;
        private readonly Mock<IHttpContextAccessor> _httpContextMock;
        private readonly API.Features.v2.Auth.TokenController _sut;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public TokenControllerUnitTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<IdentityDataStore> database)
        {
            _outputHelper = outputHelper;
            _mediatorMock = new Mock<IMediator>(Strict);

            _unitOfWorkFactory = new EFUnitOfWorkFactory<IdentityDataStore>(database.OptionsBuilder.Options, (options) =>
            {
                IdentityDataStore context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();

                return context;
            });

            _jwtOptionsMock = new Mock<IOptionsMonitor<JwtOptions>>(Strict);
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
            _jwtOptionsMock.Setup(mock => mock.CurrentValue)
                .Returns(_jwtOptions);
            _httpContextMock = new Mock<IHttpContextAccessor>(Strict);
            _sut = new API.Features.v2.Auth.TokenController(mediator: _mediatorMock.Object, jwtOptions: _jwtOptionsMock.Object, _httpContextMock.Object);
        }

        [Fact]
        public async Task GivenAccountDoesNotExist_Post_Returns_NotFound()
        {
            // Arrange
            LoginModel model = new() { Username = "Bruce", Password = Password.From("CapedCrusader") };
            _mediatorMock.Setup(mock => mock.Send(It.IsNotNull<GetOneAccountByUsernameAndPasswordQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.None<AccountInfo>());

            // Act
            ActionResult<BearerTokenInfo> actionResult = await _sut.Post(model, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsNotNull<GetOneAccountByUsernameAndPasswordQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetOneAccountByUsernameAndPasswordQuery>(q => q.Data.UserName == UserName.From(model.Username)
                                                                                                       && q.Data.Password == model.Password), It.IsAny<CancellationToken>()), Times.Once);
            _jwtOptionsMock.Verify(mock => mock.CurrentValue, Times.Never);

            actionResult.Should()
                .NotBeNull();
            actionResult.Result.Should()
                .BeAssignableTo<NotFoundResult>("The account with the specified credentials was not found");
        }

        [Fact]
        public async Task GivenAccountExists_Post_Returns_ValidToken()
        {
            // Arrange
            LoginModel model = new() { Username = "Bruce", Password = Password.From("CapedCrusader") };
            AuthenticationInfo authenticationInfo = new() { Location = "Paris" };

            DateTime accessTokenExpiresDate = 10.January(2010).Add(12.Hours()).ToUniversalTime();
            DateTime refreshTokenExpiresDate = 10.January(2010).Add(23.Hours().And(59.Minutes().And(59.Seconds()))).ToUniversalTime();

            AccountInfo accountInfo = new()
            {
                Id = AccountId.New(),
                Username = UserName.From(model.Username),
                Email = Email.From("brucewayne@gotham.com"),
                Name = "Bruce Wayne"
            };

            _mediatorMock.Setup(mock => mock.Send(It.IsNotNull<GetOneAccountByUsernameAndPasswordQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.Some(accountInfo));
            _mediatorMock.Setup(mock => mock.Send(It.IsNotNull<CreateAuthenticationTokenCommand>(), It.IsAny<CancellationToken>()))
                .Returns((CreateAuthenticationTokenCommand cmd, CancellationToken _) =>
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
            ActionResult<BearerTokenInfo> actionResult = await _sut.Post(model, new[] { authenticationInfo.Location }, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsNotNull<GetOneAccountByUsernameAndPasswordQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetOneAccountByUsernameAndPasswordQuery>(q => q.Data.UserName == UserName.From(model.Username)
                                                                                                       && q.Data.Password == model.Password), It.IsAny<CancellationToken>()), Times.Once);

            _mediatorMock.Verify(mock => mock.Send(It.IsNotNull<CreateAuthenticationTokenCommand>(), It.IsAny<CancellationToken>()));
            _mediatorMock.Verify(mock => mock.Send(It.Is<CreateAuthenticationTokenCommand>(cmd => cmd.Data.authInfo.Location == authenticationInfo.Location
                && cmd.Data.jwtInfos.Issuer == _jwtOptions.Issuer
                && cmd.Data.jwtInfos.Key == _jwtOptions.Key
                && cmd.Data.jwtInfos.Audiences.All(audience => _jwtOptions.Audiences.Contains(audience))
                && cmd.Data.jwtInfos.AccessTokenLifetime == _jwtOptions.AccessTokenLifetime
                && cmd.Data.jwtInfos.RefreshTokenLifetime == _jwtOptions.RefreshTokenLifetime), It.IsAny<CancellationToken>()));

            _jwtOptionsMock.Verify(mock => mock.CurrentValue, Times.Once);

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
