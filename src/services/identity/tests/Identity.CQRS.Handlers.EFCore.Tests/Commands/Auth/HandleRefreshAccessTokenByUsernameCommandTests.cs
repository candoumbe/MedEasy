using Bogus;
using FluentAssertions;
using FluentAssertions.Extensions;
using Identity.CQRS.Commands;
using Identity.CQRS.Handlers;
using Identity.CQRS.Handlers.Commands;
using Identity.DataStores.SqlServer;
using Identity.DTO;
using Identity.Objects;
using MedEasy.Abstractions;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Moq;
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

namespace Identity.CQRS.UnitTests.Handlers.Commands.Auth
{
    [UnitTest]
    [Feature("JWT")]
    public class HandleRefreshAccessTokenByUsernameCommandTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _uowFactory;
        private readonly SigningCredentials _signingCredentials;
        private Mock<IDateTimeService> _datetimeServiceMock;
        private Mock<IHandleCreateSecurityTokenCommand> _handleCreateSecurityTokenMock;
        private HandleRefreshAccessTokenByUsernameCommand _sut;
        private JwtSecurityTokenHandler _jwtSecurityTokenHandler;
        private const string _signatureKey = "a_very_long_key_to_encrypt_token";

        public HandleRefreshAccessTokenByUsernameCommandTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture databaseFixture)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<IdentityContext> dbContextBuilderOptionsBuilder = new DbContextOptionsBuilder<IdentityContext>()
                .UseSqlite(databaseFixture.Connection);

            _uowFactory = new EFUnitOfWorkFactory<IdentityContext>(dbContextBuilderOptionsBuilder.Options, (options) =>
            {
                IdentityContext context = new IdentityContext(options);
                context.Database.EnsureCreated();
                return context;
            });

            _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            _signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signatureKey)), SecurityAlgorithms.HmacSha256);
            _datetimeServiceMock = new Mock<IDateTimeService>(Strict);
            _handleCreateSecurityTokenMock = new Mock<IHandleCreateSecurityTokenCommand>(Strict);
            _handleCreateSecurityTokenMock.Setup(mock => mock.Handle(It.IsAny<CreateSecurityTokenCommand>(), It.IsAny<CancellationToken>()))
                .Returns((CreateSecurityTokenCommand cmd, CancellationToken ct) =>
                {
                    (JwtSecurityTokenOptions tokenOptions, IEnumerable<ClaimInfo> claims) = cmd.Data;
                    SecurityToken st = new JwtSecurityToken(
                        signingCredentials: _signingCredentials,
                        issuer: tokenOptions.Issuer,
                        claims: claims.Select(claim => new Claim(claim.Type, claim.Value))
                            .Concat(tokenOptions.Audiences.Select(aud => new Claim(JwtRegisteredClaimNames.Aud, aud)))
                    );

                    return Task.FromResult(st);
                });
            _sut = new HandleRefreshAccessTokenByUsernameCommand(datetimeService: _datetimeServiceMock.Object, uowFactory: _uowFactory, _handleCreateSecurityTokenMock.Object);
        }

        public async void Dispose()
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
            _datetimeServiceMock = null;
            _sut = null;
            _jwtSecurityTokenHandler = null;
        }

        [Fact]
        public async Task GivenExpiredRefreshToken_Handler_Returns_Unauthorized()
        {
            // Arrange
            DateTime utcNow = 25.June(2018).Add(15.Hours());

            Faker faker = new Faker();

            JwtInfos tokenInfos = new JwtInfos
            {
                Issuer = faker.Internet.DomainName(),
                Key = faker.Lorem.Word(),
                AccessTokenLifetime = faker.Random.Int(min: 1, max: 10),
                RefreshTokenLifetime = faker.Random.Int(min: 10, max: 20),
                Audiences = faker.Lorem.Words()
            };
            SecurityToken accessToken = new JwtSecurityToken(
                audience: "api",
                notBefore: utcNow.Subtract(2.Days()),
                expires: utcNow.Subtract(2.Days()).Add(1.Hours()),
                signingCredentials: _signingCredentials,
                claims: new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }
            );
            SecurityToken refreshToken = new JwtSecurityToken(
                audience : "api",
                notBefore: utcNow.Subtract(2.Days()),
                expires: utcNow.Subtract(1.Days()),
                signingCredentials: _signingCredentials,
                claims: new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }
            );

            string refreshTokenString = _jwtSecurityTokenHandler.WriteToken(refreshToken);
            string expiredAccessTokenString = _jwtSecurityTokenHandler.WriteToken(accessToken);

            _outputHelper.WriteLine($"Refresh token : {refreshTokenString}");

            RefreshAccessTokenByUsernameCommand cmd = new RefreshAccessTokenByUsernameCommand(("thejoker", expiredAccessToken: expiredAccessTokenString, refreshTokenString, tokenInfos));

            _datetimeServiceMock.Setup(mock => mock.UtcNow()).Returns(utcNow);

            // Act
            Option<BearerTokenInfo, RefreshAccessCommandResult> optionalBearer = await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            _datetimeServiceMock.Verify(mock => mock.UtcNow(), Times.Once);
            _handleCreateSecurityTokenMock.Verify(mock => mock.Handle(It.IsAny<CreateSecurityTokenCommand>(), It.IsAny<CancellationToken>()), Times.Never);

            optionalBearer.HasValue.Should()
                .BeFalse();
            optionalBearer.MatchNone(cmdResult => cmdResult.Should().Be(RefreshAccessCommandResult.Unauthorized, "The refresh token is expired"));
        }

        [Fact]
        public async Task GivenValidRefreshToken_Handler_Returns_NewToken()
        {
            // Arrange
            DateTime utcNow = 25.June(2018).Add(15.Hours());
            Faker faker = new Faker();
            JwtSecurityToken refreshToken = new JwtSecurityToken(
               audience: "api",
               notBefore: utcNow.Subtract(2.Days()),
               expires: utcNow.Add(1.Days()),
               signingCredentials: _signingCredentials,
               claims: new[]
               {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
               }
           );
            Account account = new Account
            (
                uuid: Guid.NewGuid(),
                name : faker.Person.FullName,
                email : faker.Person.Email,
                passwordHash : faker.Lorem.Word(),
                salt : faker.Lorem.Word(),
                username : faker.Person.UserName
            );
            account.ChangeRefreshToken(new JwtSecurityTokenHandler().WriteToken(refreshToken));

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Create(account);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            JwtInfos tokenOptions = new JwtInfos
            {
                Issuer = faker.Internet.DomainName(),
                Key = faker.Lorem.Word(),
                AccessTokenLifetime = faker.Random.Int(min: 1, max: 10),
                RefreshTokenLifetime = faker.Random.Int(min: 20, max: 30),
                Audiences = faker.Lorem.Words()
            };
            SecurityToken accessToken = new JwtSecurityToken(
                audience: "api",
                notBefore: utcNow.Subtract(2.Days()),
                expires: utcNow.Add(2.Days()),
                signingCredentials: _signingCredentials,
                claims: new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }
            );
            _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            string refreshTokenString = _jwtSecurityTokenHandler.WriteToken(refreshToken);
            string expiredAccessTokenString = _jwtSecurityTokenHandler.WriteToken(refreshToken);

            _outputHelper.WriteLine($"Refresh token : {refreshTokenString}");

            RefreshAccessTokenByUsernameCommand refreshAccessTokenByUsernameCommand = new RefreshAccessTokenByUsernameCommand((account.Username, expiredAccessTokenString, refreshTokenString, tokenOptions));

            _datetimeServiceMock.Setup(mock => mock.UtcNow()).Returns(utcNow);

            // Act
            Option<BearerTokenInfo, RefreshAccessCommandResult> optionalBearer = await _sut.Handle(refreshAccessTokenByUsernameCommand, default)
                .ConfigureAwait(false);

            // Assert
            optionalBearer.HasValue.Should()
                .BeTrue("refresh token is valid and the user exists in the database");
            _datetimeServiceMock.Verify(mock => mock.UtcNow(), Times.Once);
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
