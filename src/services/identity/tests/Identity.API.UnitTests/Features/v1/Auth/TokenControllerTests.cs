namespace Identity.API.UnitTests.Features.v1.Auth
{
    using Bogus;

    using FluentAssertions;

    using FsCheck;
    using FsCheck.Xunit;

    using Identity.API.Features.Auth;
    using Identity.API.Features.v1.Auth;
    using Identity.CQRS.Commands;
    using Identity.CQRS.Commands.v1;
    using Identity.DataStores;
    using Identity.DTO.Auth;
    using Identity.DTO.v1;
    using MedEasy.ValueObjects;

    using MedEasy.CQRS.Core.Commands.Results;
    using MedEasy.DAL.EFStore;
    using MedEasy.DAL.Interfaces;
    using MedEasy.IntegrationTests.Core;

    using MediatR;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;

    using Moq;

    using NodaTime;
    using NodaTime.Testing;

    using Optional;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
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
    public class TokenControllerUnitTests : IClassFixture<SqliteEfCoreDatabaseFixture<IdentityDataStore>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IOptionsSnapshot<JwtOptions>> _jwtOptionsMock;
        private readonly JwtOptions _jwtOptions;
        private readonly Mock<IHttpContextAccessor> _httpContextMock;
        private readonly TokenController _sut;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private static readonly Faker Faker = new ();

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

        /// <summary>
        /// Given a valid request when the user is not found then a NotFoundResult is returned.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GivenAccountDoesNotExist_Post_Returns_NotFound()
        {
            // Arrange
            LoginModel model = new() { Username = "Bruce", Password = Password.From("CapedCrusader") };
            string forwardedFor = Faker.Internet.IpAddress().ToString();

            _mediatorMock.Setup(mock => mock.Send(It.IsNotNull<LoginCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(Option.None<BearerTokenInfo>());
            _jwtOptionsMock.SetupGet(mock => mock.Value).Returns(_jwtOptions);
            _httpContextMock.SetupGet(mock => mock.HttpContext.Request.Headers).Returns(new HeaderDictionary()
            {
                ["X-FORWARDED-FOR"] = new StringValues(forwardedFor)
            });

            // Act
            ActionResult<BearerTokenInfo> actionResult = await _sut.Post(model, ct: default)
                                                                   .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.Is<LoginCommand>(q => q.Data.LoginInfos.UserName == UserName.From(model.Username)
                                                                            && q.Data.LoginInfos.Password == model.Password
                                                                            && q.Data.JwtInfos.Audiences == _jwtOptions.Audiences
                                                                            && q.Data.JwtInfos.AccessTokenLifetime == _jwtOptions.AccessTokenLifetime
                                                                            && q.Data.JwtInfos.Issuer == _jwtOptions.Issuer
                                                                            && q.Data.JwtInfos.Key == _jwtOptions.Key
                                                                            && q.Data.JwtInfos.RefreshTokenLifetime == _jwtOptions.RefreshTokenLifetime
                                                                            && q.Data.Location == forwardedFor),
                                                   It.IsAny<CancellationToken>()), Times.Once);

            _mediatorMock.VerifyNoOtherCalls();

            _jwtOptionsMock.Verify(mock => mock.Value, Times.Once);
            _jwtOptionsMock.VerifyNoOtherCalls();

            _httpContextMock.VerifyGet(mock => mock.HttpContext.Request.Headers, Times.Once);
            _httpContextMock.VerifyNoOtherCalls();

            actionResult.Result.Should()
                        .BeAssignableTo<NotFoundResult>("The account with the specified credentials was not found");
        }

        /// <summary>
        /// Given an account exists and the password is correct, the controller should return a valid token.
        /// </summary>
        /// <returns></returns>
        [Property]
        public async Task GivenAccountExists_Post_Returns_ValidToken(NonWhiteSpaceString accessToken, NonWhiteSpaceString refreshToken)
        {
            // Arrange
            LoginModel model = new() { Username = "Bruce", Password = Password.From("CapedCrusader") };
            string forwardedFor = Faker.Internet.IpAddress().ToString();
            BearerTokenInfo bearerTokenInfo = new()
            {
                AccessToken = accessToken.Get,
                RefreshToken = refreshToken.Get
            };
            _mediatorMock.Setup(mock => mock.Send(It.IsNotNull<LoginCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(bearerTokenInfo.Some());
            
            _jwtOptionsMock.SetupGet(mock => mock.Value).Returns(_jwtOptions);
            _httpContextMock.SetupGet(mock => mock.HttpContext.Request.Headers).Returns(new HeaderDictionary()
            {
                ["X-FORWARDED-FOR"] = new StringValues(forwardedFor)
            });

            // Act
            ActionResult<BearerTokenInfo> actionResult = await _sut.Post(model, ct: default)
                                                                   .ConfigureAwait(false);

            // Assert
            BearerTokenInfo bearerToken = actionResult.Value.Should()
                                                            .BeOfType<BearerTokenInfo>().Which;

            bearerToken.AccessToken.Should()
                                   .Be(accessToken.Item);

            bearerToken.RefreshToken.Should()
                                    .Be(refreshToken.Item);

            _mediatorMock.Verify(mock => mock.Send(It.Is<LoginCommand>(q => q.Data.LoginInfos.UserName == UserName.From(model.Username)
                                                                            && q.Data.LoginInfos.Password == model.Password
                                                                            && q.Data.JwtInfos.Audiences == _jwtOptions.Audiences
                                                                            && q.Data.JwtInfos.AccessTokenLifetime == _jwtOptions.AccessTokenLifetime
                                                                            && q.Data.JwtInfos.Issuer == _jwtOptions.Issuer
                                                                            && q.Data.JwtInfos.Key == _jwtOptions.Key
                                                                            && q.Data.JwtInfos.RefreshTokenLifetime == _jwtOptions.RefreshTokenLifetime
                                                                            && q.Data.Location == forwardedFor),
                                                   It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.VerifyNoOtherCalls();

            _jwtOptionsMock.Verify(mock => mock.Value);
            _jwtOptionsMock.VerifyNoOtherCalls();

            _httpContextMock.VerifyGet(mock => mock.HttpContext.Request.Headers);
            _httpContextMock.VerifyNoOtherCalls();
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
            UserName username = UserName.From("thejoker");
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<InvalidateAccessTokenByUsernameCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cmdResult);

            // Act
            IActionResult actionResult = await _sut.Invalidate(username.Value, ct: default)
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
            UserName username = UserName.From("thejoker");
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<RefreshAccessTokenByUsernameCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.None<BearerTokenInfo, RefreshAccessCommandResult>(RefreshAccessCommandResult.NotFound));
            RefreshAccessTokenInfo refreshAccessToken = new()
            {
                AccessToken = "access",
                RefreshToken = "refresh"
            };

            // Act
            IActionResult actionResult = await _sut.Refresh(username.Value, refreshAccessToken, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<RefreshAccessTokenByUsernameCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<RefreshAccessTokenByUsernameCommand>(cmd => cmd.Data.username == username
                                                                                                     && cmd.Data.refreshToken == refreshAccessToken.RefreshToken), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task GivenMediator_Returns_Conflict_Refresh_Returns_Conflict()
        {
            // Arrange
            UserName username = UserName.From("thejoker");
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<RefreshAccessTokenByUsernameCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.None<BearerTokenInfo, RefreshAccessCommandResult>(RefreshAccessCommandResult.Conflict));
            RefreshAccessTokenInfo refreshAccessToken = new()
            {
                AccessToken = "access",
                RefreshToken = "refresh"
            };

            // Act
            IActionResult actionResult = await _sut.Refresh(username.Value, refreshAccessToken, ct: default)
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
            UserName username = UserName.From("thejoker");
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
            IActionResult actionResult = await _sut.Refresh(username.Value, refreshAccessToken, ct: default)
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
            UserName username = UserName.From("thejoker");

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<RefreshAccessTokenByUsernameCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.None<BearerTokenInfo, RefreshAccessCommandResult>(RefreshAccessCommandResult.Unauthorized));
            RefreshAccessTokenInfo refreshAccessToken = new()
            {
                AccessToken = "access",
                RefreshToken = "refresh"
            };

            // Act
            IActionResult actionResult = await _sut.Refresh(username.Value, refreshAccessToken, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<RefreshAccessTokenByUsernameCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<RefreshAccessTokenByUsernameCommand>(cmd => cmd.Data.username == username && cmd.Data.refreshToken == refreshAccessToken.RefreshToken), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<UnauthorizedResult>();
        }
    }
}
