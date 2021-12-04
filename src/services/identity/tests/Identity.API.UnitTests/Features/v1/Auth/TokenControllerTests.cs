namespace Identity.API.UnitTests.Features.v1.Auth
{
    using FluentAssertions;

    using Identity.API.Features.Auth;
    using Identity.API.Features.v1.Auth;
    using Identity.CQRS.Commands;
    using Identity.CQRS.Queries.Accounts;
    using Identity.DataStores;
    using Identity.DTO;
    using Identity.DTO.Auth;
    using Identity.DTO.v1;
    using Identity.Ids;

    using MedEasy.CQRS.Core.Commands.Results;
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

    using NodaTime;
    using NodaTime.Testing;

    using Optional;

    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    using static Microsoft.AspNetCore.Http.StatusCodes;
    using static Moq.MockBehavior;

    [UnitTest]
    [Feature("Identity")]
    [Feature("Accounts")]
    public class TokenControllerUnitTests : IClassFixture<SqliteEfCoreDatabaseFixture<IdentityContext>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IOptionsSnapshot<JwtOptions>> _jwtOptionsMock;
        private readonly JwtOptions _jwtOptions;
        private readonly Mock<IHttpContextAccessor> _httpContextMock;
        private readonly TokenController _sut;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public TokenControllerUnitTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<IdentityContext> database)
        {
            _outputHelper = outputHelper;
            _mediatorMock = new Mock<IMediator>(Strict);

            _unitOfWorkFactory = new EFUnitOfWorkFactory<IdentityContext>(database.OptionsBuilder.Options, (options) =>
            {
                IdentityContext context = new(options, new FakeClock(new Instant()));
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

        [Fact]
        public async Task GivenAccountDoesNotExist_Post_Returns_NotFound()
        {
            // Arrange
            LoginModel model = new() { Username = "Bruce", Password = "CapedCrusader" };
            _mediatorMock.Setup(mock => mock.Send(It.IsNotNull<GetOneAccountByUsernameAndPasswordQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.None<AccountInfo>());

            // Act
            IActionResult actionResult = await _sut.Post(model, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsNotNull<GetOneAccountByUsernameAndPasswordQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetOneAccountByUsernameAndPasswordQuery>(q => q.Data.Username == model.Username && q.Data.Password == model.Password), It.IsAny<CancellationToken>()), Times.Once);
            _jwtOptionsMock.Verify(mock => mock.Value, Times.Never);

            actionResult.Should()
                .BeAssignableTo<NotFoundResult>("The account with the specified credentials was not found");
        }

        [Fact]
        public async Task GivenAccountExists_Post_Returns_ValidToken()
        {
            // Arrange
            LoginModel model = new() { Username = "Bruce", Password = "CapedCrusader" };
            AuthenticationInfo authenticationInfo = new() { Location = "Paris" };
            AccountInfo accountInfo = new()
            {
                Id = AccountId.New(),
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
                                claims: jwtInfos.Audiences.Select(aud => new Claim(JwtRegisteredClaimNames.Aud, aud))
                            ),
                            RefreshToken = new JwtSecurityToken(
                                issuer: _jwtOptions.Issuer,
                                claims: jwtInfos.Audiences
                                    .Select(aud => new Claim(JwtRegisteredClaimNames.Aud, aud))
                                    .Concat(new[] { new Claim(CustomClaimTypes.Location, authInfo.Location) })
                            ),
                        });
                    }
                  );

            // Act
            IActionResult actionResult = await _sut.Post(model, ct: default)
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

            BearerTokenInfo bearerToken = actionResult.Should()
                .BeOfType<OkObjectResult>().Which
                .Value.Should()
                .BeOfType<BearerTokenInfo>().Which;

            bearerToken.AccessToken.Should()
                .NotBeNullOrWhiteSpace();

            bearerToken.RefreshToken.Should()
                .NotBeNullOrWhiteSpace().And
                .NotBe(bearerToken.AccessToken);

            SecurityToken accessToken = new JwtSecurityToken(bearerToken.AccessToken);

        }

        public static IEnumerable<object[]> InvalidateCases
        {
            get
            {
                yield return new object[]
                {
                    InvalidateAccessCommandResult.Done,
                    (Expression<Func<IActionResult, bool>>)(actionResult => actionResult is NoContentResult),
                    "The command completed successfully"
                };

                yield return new object[]
                {
                    InvalidateAccessCommandResult.Failed_Conflict,
                    (Expression<Func<IActionResult, bool>>)(actionResult => actionResult is StatusCodeResult && ((StatusCodeResult)actionResult).StatusCode == Status409Conflict),
                    $"The command returned <{nameof(InvalidateAccessCommandResult.Failed_Conflict)}>"
                };

                yield return new object[]
                {
                    InvalidateAccessCommandResult.Failed_NotFound,
                    (Expression<Func<IActionResult, bool>>)(actionResult => actionResult is NotFoundResult),
                    $"The command returned <{nameof(InvalidateAccessCommandResult.Failed_NotFound)}>"
                };

                yield return new object[]
                {
                    InvalidateAccessCommandResult.Failed_Unauthorized,
                    (Expression<Func<IActionResult, bool>>)(actionResult => actionResult is UnauthorizedResult),
                    $"The command returned <{nameof(InvalidateAccessCommandResult.Failed_Unauthorized)}>"
                };
            }
        }

        [Theory]
        [MemberData(nameof(InvalidateCases))]
        public async Task Invalidate(InvalidateAccessCommandResult cmdResult, Expression<Func<IActionResult, bool>> actionResultExpectation, string reason)
        {
            // Arrange
            const string username = "thejoker";
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<InvalidateAccessTokenByUsernameCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cmdResult);

            // Act
            IActionResult actionResult = await _sut.Invalidate(username, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<InvalidateAccessTokenByUsernameCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<InvalidateAccessTokenByUsernameCommand>(cmd => cmd.Data == username), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .Match(actionResultExpectation, reason);
        }

        [Fact]
        public void InvalidateIsEndpoint()
        {
            MethodInfo invalidateMethod = typeof(TokenController).GetMethod(nameof(TokenController.Invalidate));
            invalidateMethod.Should()
                .BeAsync().And
                .BeDecoratedWith<HttpDeleteAttribute>().Which
                .Template.Should()
                .Be("{username}");

            ParameterInfo[] parameters = invalidateMethod.GetParameters();
            parameters.Should()
                .ContainSingle(pi => pi.ParameterType == typeof(CancellationToken) && pi.IsOptional);
        }

        [Fact]
        public void RefreshIsEndpoint()
        {
            MethodInfo invalidateMethod = typeof(TokenController).GetMethod(nameof(TokenController.Refresh));
            invalidateMethod.Should()
                .BeAsync().And
                .BeDecoratedWith<HttpPutAttribute>().Which
                .Template.Should()
                .Be("{username}");

            ParameterInfo[] parameters = invalidateMethod.GetParameters();
            parameters.Should()
                .ContainSingle(pi => pi.ParameterType == typeof(CancellationToken) && pi.IsOptional).And
                .ContainSingle(pi => pi.ParameterType == typeof(RefreshAccessTokenInfo));

            ParameterInfo refreshAccessTokenParameter = parameters.Single(pi => pi.ParameterType == typeof(RefreshAccessTokenInfo));
            IEnumerable<Attribute> attributes = refreshAccessTokenParameter.GetCustomAttributes();
            attributes.Should()
                .ContainSingle(attr => attr is FromBodyAttribute);
        }

        [Fact]
        public async Task GivenMediator_Returns_NotFound_Refresh_Returns_NotFoundResult()
        {
            // Arrange
            const string username = "thejoker";
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<RefreshAccessTokenByUsernameCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.None<BearerTokenInfo, RefreshAccessCommandResult>(RefreshAccessCommandResult.NotFound));
            RefreshAccessTokenInfo refreshAccessToken = new()
            {
                AccessToken = "access",
                RefreshToken = "refresh"
            };

            // Act
            IActionResult actionResult = await _sut.Refresh(username, refreshAccessToken, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<RefreshAccessTokenByUsernameCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<RefreshAccessTokenByUsernameCommand>(cmd => cmd.Data.username == username && cmd.Data.refreshToken == refreshAccessToken.RefreshToken), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task GivenMediator_Returns_Conflict_Refresh_Returns_Conflict()
        {
            // Arrange
            const string username = "thejoker";
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<RefreshAccessTokenByUsernameCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.None<BearerTokenInfo, RefreshAccessCommandResult>(RefreshAccessCommandResult.Conflict));
            RefreshAccessTokenInfo refreshAccessToken = new()
            {
                AccessToken = "access",
                RefreshToken = "refresh"
            };

            // Act
            IActionResult actionResult = await _sut.Refresh(username, refreshAccessToken, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<RefreshAccessTokenByUsernameCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<RefreshAccessTokenByUsernameCommand>(cmd => cmd.Data.username == username && cmd.Data.refreshToken == refreshAccessToken.RefreshToken), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<StatusCodeResult>().Which
                .StatusCode.Should()
                .Be(Status409Conflict);
        }

        [Fact]
        public async Task GivenMediator_Returns_Bearer_Refresh_Returns_NewTokens()
        {
            // Arrange
            const string username = "thejoker";
            BearerTokenInfo bearerToken = new()
            {
                AccessToken = "<header-access>.<payload>.<signature>",
                RefreshToken = "<header-refresh>.<payload>.<signature>"
            };
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<RefreshAccessTokenByUsernameCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.Some<BearerTokenInfo, RefreshAccessCommandResult>(bearerToken));
            RefreshAccessTokenInfo refreshAccessToken = new()
            {
                AccessToken = "access",
                RefreshToken = "refresh"
            };

            // Act
            IActionResult actionResult = await _sut.Refresh(username, refreshAccessToken, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<RefreshAccessTokenByUsernameCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<RefreshAccessTokenByUsernameCommand>(cmd => cmd.Data.username == username && cmd.Data.refreshToken == refreshAccessToken.RefreshToken), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<OkObjectResult>().Which
                .Value.Should()
                .BeSameAs(bearerToken);
        }

        [Fact]
        public async Task GivenMediator_Returns_Bearer_Unauthorized_Returns_Unauthorized()
        {
            // Arrange
            const string username = "thejoker";

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<RefreshAccessTokenByUsernameCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.None<BearerTokenInfo, RefreshAccessCommandResult>(RefreshAccessCommandResult.Unauthorized));
            RefreshAccessTokenInfo refreshAccessToken = new()
            {
                AccessToken = "access",
                RefreshToken = "refresh"
            };

            // Act
            IActionResult actionResult = await _sut.Refresh(username, refreshAccessToken, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<RefreshAccessTokenByUsernameCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<RefreshAccessTokenByUsernameCommand>(cmd => cmd.Data.username == username && cmd.Data.refreshToken == refreshAccessToken.RefreshToken), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<UnauthorizedResult>();
        }
    }
}
