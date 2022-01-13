namespace Identity.CQRS.UnitTests.Handlers.Commands.Auth
{
    using Bogus;

    using FluentAssertions;
    using FluentAssertions.Extensions;

    using Identity.CQRS.Commands;
    using Identity.CQRS.Handlers;
    using Identity.CQRS.Handlers.Commands;
    using Identity.DataStores;
    using Identity.DTO;
    using Identity.DTO.v1;
    using Identity.Ids;
    using Identity.Objects;

    using MedEasy.CQRS.Core.Commands.Results;
    using MedEasy.DAL.EFStore;
    using MedEasy.DAL.Interfaces;
    using MedEasy.IntegrationTests.Core;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;

    using Moq;

    using NodaTime;
    using NodaTime.Extensions;
    using NodaTime.Testing;

    using Optional;

    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    using static Moq.MockBehavior;

    using Claim = System.Security.Claims.Claim;

    [UnitTest]
    [Feature("JWT")]
    public class HandleRefreshAccessTokenByUsernameCommandTests : IAsyncLifetime, IClassFixture<SqliteEfCoreDatabaseFixture<IdentityContext>>
    {
        private ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _uowFactory;
        private readonly SigningCredentials _signingCredentials;
        private Mock<IClock> _clockMock;
        private Mock<IHandleCreateSecurityTokenCommand> _handleCreateSecurityTokenMock;
        private HandleRefreshAccessTokenByUsernameCommand _sut;
        private JwtSecurityTokenHandler _jwtSecurityTokenHandler;
        private const string SignatureKey = "a_very_long_key_to_encrypt_token";

        public HandleRefreshAccessTokenByUsernameCommandTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<IdentityContext> databaseFixture)
        {
            _outputHelper = outputHelper;
            _uowFactory = new EFUnitOfWorkFactory<IdentityContext>(databaseFixture.OptionsBuilder.Options, (options) =>
            {
                IdentityContext context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });

            _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            _signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SignatureKey)), SecurityAlgorithms.HmacSha256);
            _clockMock = new(Strict);
            _handleCreateSecurityTokenMock = new Mock<IHandleCreateSecurityTokenCommand>(Strict);
            _handleCreateSecurityTokenMock.Setup(mock => mock.Handle(It.IsAny<CreateSecurityTokenCommand>(), It.IsAny<CancellationToken>()))
                .Returns((CreateSecurityTokenCommand cmd, CancellationToken ct) =>
                {
                    (JwtSecurityTokenOptions tokenOptions, _, IEnumerable<ClaimInfo> claims) = cmd.Data;
                    SecurityToken st = new JwtSecurityToken(
                        signingCredentials: _signingCredentials,
                        issuer: tokenOptions.Issuer,
                        claims: claims.Select(claim => new Claim(claim.Type, claim.Value))
                            .Concat(tokenOptions.Audiences.Select(aud => new Claim(JwtRegisteredClaimNames.Aud, aud)))
                    );

                    return Task.FromResult(st);
                });
            _sut = new HandleRefreshAccessTokenByUsernameCommand(datetimeService: _clockMock.Object,
                                                                 uowFactory: _uowFactory,
                                                                 _handleCreateSecurityTokenMock.Object);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            _outputHelper = null;
            _handleCreateSecurityTokenMock = null;
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Clear();
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            _uowFactory = null;
            _clockMock = null;
            _sut = null;
            _jwtSecurityTokenHandler = null;
        }

        [Fact]
        public async Task GivenExpiredRefreshToken_Handler_Returns_Unauthorized()
        {
            // Arrange
            Instant utcNow = 25.June(2018).Add(15.Hours()).AsUtc().ToInstant();

            Faker faker = new();

            JwtInfos tokenInfos = new()
            {
                Issuer = faker.Internet.DomainName(),
                Key = faker.Lorem.Word(),
                AccessTokenLifetime = faker.Random.Int(min: 1, max: 10),
                RefreshTokenLifetime = faker.Random.Int(min: 10, max: 20),
                Audiences = faker.Lorem.Words()
            };
            SecurityToken accessToken = new JwtSecurityToken(
                audience: "api",
                notBefore: utcNow.Minus(2.Days().ToDuration()).ToDateTimeUtc(),
                expires: utcNow.Minus(2.Days().ToDuration()).Plus(1.Hours().ToDuration()).ToDateTimeUtc(),
                signingCredentials: _signingCredentials,
                claims: new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }
            );
            SecurityToken refreshToken = new JwtSecurityToken(
                audience: "api",
                notBefore: utcNow.Minus(2.Days().ToDuration()).ToDateTimeUtc(),
                expires: utcNow.Minus(1.Days().ToDuration()).ToDateTimeUtc(),
                signingCredentials: _signingCredentials,
                claims: new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }
            );

            string refreshTokenString = _jwtSecurityTokenHandler.WriteToken(refreshToken);
            string expiredAccessTokenString = _jwtSecurityTokenHandler.WriteToken(accessToken);

            _outputHelper.WriteLine($"Refresh token : {refreshTokenString}");

            RefreshAccessTokenByUsernameCommand cmd = new(("thejoker", expiredAccessToken: expiredAccessTokenString, refreshTokenString, tokenInfos));

            _clockMock.Setup(mock => mock.GetCurrentInstant())
                      .Returns(utcNow);

            // Act
            Option<BearerTokenInfo, RefreshAccessCommandResult> optionalBearer = await _sut.Handle(cmd, default)
                                                                                           .ConfigureAwait(false);

            // Assert
            _clockMock.Verify(mock => mock.GetCurrentInstant(), Times.Once);
            _handleCreateSecurityTokenMock.Verify(mock => mock.Handle(It.IsAny<CreateSecurityTokenCommand>(), It.IsAny<CancellationToken>()), Times.Never);

            optionalBearer.HasValue.Should()
                                   .BeFalse();
            optionalBearer.MatchNone(cmdResult => cmdResult.Should().Be(RefreshAccessCommandResult.Unauthorized, "The refresh token is expired"));
        }

        [Fact]
        public async Task GivenValidRefreshToken_Handler_Returns_NewToken()
        {
            // Arrange
            Instant utcNow = 25.June(2018).Add(15.Hours()).AsUtc().ToInstant();
            Faker faker = new();
            JwtSecurityToken refreshToken = new(
               audience: "api",
               notBefore: utcNow.Minus(2.Days().ToDuration()).ToDateTimeUtc(),
               expires: utcNow.Plus(1.Days().ToDuration()).ToDateTimeUtc(),
               signingCredentials: _signingCredentials,
               claims: new[]
               {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
               }
           );
            Account account = new(id: AccountId.New(),
                                   name: faker.Person.FullName,
                                   email: faker.Person.Email,
                                   passwordHash: faker.Lorem.Word(),
                                   salt: faker.Lorem.Word(),
                                   username: faker.Person.UserName);

            account.ChangeRefreshToken(new JwtSecurityTokenHandler().WriteToken(refreshToken));

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Create(account);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            JwtInfos tokenOptions = new()
            {
                Issuer = faker.Internet.DomainName(),
                Key = faker.Lorem.Word(),
                AccessTokenLifetime = faker.Random.Int(min: 1, max: 10),
                RefreshTokenLifetime = faker.Random.Int(min: 20, max: 30),
                Audiences = faker.Lorem.Words()
            };

            _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            string refreshTokenString = _jwtSecurityTokenHandler.WriteToken(refreshToken);
            string expiredAccessTokenString = _jwtSecurityTokenHandler.WriteToken(refreshToken);

            _outputHelper.WriteLine($"Refresh token : {refreshTokenString}");

            RefreshAccessTokenByUsernameCommand refreshAccessTokenByUsernameCommand = new((account.Username, expiredAccessTokenString, refreshTokenString, tokenOptions));

            _clockMock.Setup(mock => mock.GetCurrentInstant())
                      .Returns(utcNow);

            // Act
            Option<BearerTokenInfo, RefreshAccessCommandResult> optionalBearer = await _sut.Handle(refreshAccessTokenByUsernameCommand, default)
                .ConfigureAwait(false);

            // Assert
            optionalBearer.HasValue.Should()
                                   .BeTrue("refresh token is valid and the user exists in the database");
            _clockMock.Verify(mock => mock.GetCurrentInstant(), Times.Once);
            _handleCreateSecurityTokenMock.Verify(mock => mock.Handle(It.IsAny<CreateSecurityTokenCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _handleCreateSecurityTokenMock.Verify(mock =>
                mock.Handle(It.Is<CreateSecurityTokenCommand>(cmd => cmd.Data.tokenOptions.Audiences.SequenceEqual(tokenOptions.Audiences)
                    && cmd.Data.tokenOptions.Issuer == tokenOptions.Issuer
                    && cmd.Data.tokenOptions.Key == tokenOptions.Key
                    && cmd.Data.tokenOptions.LifetimeInMinutes == tokenOptions.AccessTokenLifetime
            ), It.IsAny<CancellationToken>()), Times.Once);

            _handleCreateSecurityTokenMock.Verify(mock =>
                mock.Handle(It.Is<CreateSecurityTokenCommand>(cmd => cmd.Data.tokenOptions.Audiences.SequenceEqual(tokenOptions.Audiences)
                    && cmd.Data.tokenOptions.Issuer == tokenOptions.Issuer
                    && cmd.Data.tokenOptions.Key == tokenOptions.Key
                    && cmd.Data.tokenOptions.LifetimeInMinutes == tokenOptions.RefreshTokenLifetime
            ), It.IsAny<CancellationToken>()), Times.Once);

            optionalBearer.HasValue.Should()
                .BeTrue("access token was successfully renewed");
            optionalBearer.MatchSome(bearer =>
            {
                bearer.AccessToken.Should()
                    .NotBeNullOrWhiteSpace().And
                    .NotBe(expiredAccessTokenString);
                bearer.RefreshToken.Should()
                    .NotBeNullOrWhiteSpace().And
                    .Be(refreshTokenString, "no slinding expiration for now so client keeps the same refresh token");
            });
        }
    }
}
