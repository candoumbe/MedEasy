using AutoMapper;
using Bogus;
using FluentAssertions;
using FluentAssertions.Extensions;
using Identity.CQRS.Commands;
using Identity.CQRS.Handlers.Commands;
using Identity.DataStores.SqlServer;
using Identity.DTO;
using Identity.Mapping;
using Identity.Objects;
using MedEasy.Abstractions;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Moq.MockBehavior;
using Claim = System.Security.Claims.Claim;

namespace Identity.CQRS.UnitTests.Handlers.Queries
{
    [UnitTest]
    [Feature("Identity")]
    [Feature("JWT")]
    [Feature("Authentication")]
    public class HandleCreateAuthenticationTokenCommandTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IDateTimeService> _dateTimeServiceMock;
        private HandleCreateAuthenticationTokenCommand _sut;
        private IUnitOfWorkFactory _uowFactory;
        private Mock<IHandleCreateSecurityTokenCommand> _handleCreateSecurityTokenCommandMock;

        public HandleCreateAuthenticationTokenCommandTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture databaseFixture)
        {
            _outputHelper = outputHelper;

            _dateTimeServiceMock = new Mock<IDateTimeService>(Strict);

            DbContextOptionsBuilder<IdentityContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<IdentityContext>();
            dbContextOptionsBuilder.UseInMemoryDatabase($"{Guid.NewGuid()}");

            _uowFactory = new EFUnitOfWorkFactory<IdentityContext>(dbContextOptionsBuilder.Options, (options) =>
            {
                IdentityContext context = new IdentityContext(options);
                
                return context;
            });
            _handleCreateSecurityTokenCommandMock = new Mock<IHandleCreateSecurityTokenCommand>(Strict);

            _sut = new HandleCreateAuthenticationTokenCommand(dateTimeService: _dateTimeServiceMock.Object, unitOfWorkFactory: _uowFactory, _handleCreateSecurityTokenCommandMock.Object);
        }

        public async void Dispose()
        {
            _outputHelper = null;
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Objects.Claim>().Clear();
                uow.Repository<Account>().Clear();
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            _handleCreateSecurityTokenCommandMock = null;
            _uowFactory = null;
            _sut = null;
            _dateTimeServiceMock = null;
        }

        [Fact]
        public void Type_Is_A_Handler() =>
            // Assert
            _sut.GetType().Should()
                .Implement<IRequestHandler<CreateAuthenticationTokenCommand, AuthenticationTokenInfo>>();


        [Fact]
        public async Task GivenAccountInfo_Handler_Returns_CorrespondingToken()
        {
            // Arrange
            DateTime utcNow = 10.January(2014).AsUtc();
            Account account = new Account
            {
                UUID = Guid.NewGuid(),
                UserName = "thebatman",
                Email = "bwayne@wayne-enterprise.com",
                Name = "Bruce Wayne",
                PasswordHash = new Faker().Lorem.Word(),
                Salt = new Faker().Lorem.Word(),
            };

            account.AddOrUpdateClaim(type: "batarangs", value: "10", utcNow);
            account.AddOrUpdateClaim(type: "fight", value: "100", utcNow);
            account.AddOrUpdateClaim(type: "money", value: "1000 K€", utcNow);

            AccountInfo accountInfo;
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Create(account);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
                accountInfo = AutoMapperConfig.Build().CreateMapper()
                    .Map<Account, AccountInfo>(account);
            }

            JwtInfos jwtInfos = new JwtInfos
            {
                Issuer = "http://localhost:10000",
                Audiences = new[] { "api1", "api2" },
                Key = "key_to_encrypt_token",
                AccessTokenLifetime = 10,
                RefreshTokenLifetime = 1.Days().TotalMinutes
            };
            _dateTimeServiceMock.Setup(mock => mock.UtcNow()).Returns(utcNow);

            _handleCreateSecurityTokenCommandMock.Setup(mock => mock.Handle(It.IsAny<CreateSecurityTokenCommand>(), It.IsAny<CancellationToken>()))
                .Returns(async (CreateSecurityTokenCommand cmd, CancellationToken ct) =>
                {
                    (JwtSecurityTokenOptions tokenOptions, IEnumerable<ClaimInfo> claims) = cmd.Data;
                    SecurityKey signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtInfos.Key));
                    SecurityToken token = new JwtSecurityToken(
                        issuer: tokenOptions.Issuer,
                        claims: claims.Select(claim => new Claim(claim.Type, claim.Value)),
                        notBefore: utcNow,
                        expires: utcNow.AddMinutes(tokenOptions.LifetimeInMinutes),
                        signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                    );

                    return await new ValueTask<SecurityToken>(token);
                });

            AuthenticationInfo authenticationInfo = new AuthenticationInfo { Location = "127.0.0.1" };
            CreateAuthenticationTokenCommand createAuthenticationTokenCommand = new CreateAuthenticationTokenCommand((authenticationInfo, accountInfo, jwtInfos));

            // Act
            AuthenticationTokenInfo authenticationToken = await _sut.Handle(createAuthenticationTokenCommand, ct: default)
                 .ConfigureAwait(false);

            // Assert
            _dateTimeServiceMock.Verify();
            _handleCreateSecurityTokenCommandMock.Verify(mock => mock.Handle(It.IsAny<CreateSecurityTokenCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(2));

            SecurityToken accessToken = authenticationToken.AccessToken;

            accessToken.Should()
                .NotBeNull();
            accessToken.Id.Should()
                .NotBeNullOrWhiteSpace().And
                .MatchRegex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", "id must be a GUID");
            accessToken.ValidFrom.Should()
                .Be(utcNow);
            accessToken.ValidTo.Should()
                .Be(utcNow.AddMinutes(jwtInfos.AccessTokenLifetime));
            accessToken.Issuer.Should()
                .Be(jwtInfos.Issuer);

            JwtSecurityToken jwtAccessToken = accessToken.Should()
                .BeOfType<JwtSecurityToken>().Which;

            jwtAccessToken.Audiences.Should()
                .HaveSameCount(jwtInfos.Audiences).And
                .OnlyHaveUniqueItems().And
                .BeEquivalentTo(jwtInfos.Audiences);

            jwtAccessToken.Claims.Should()
                .ContainSingle(claim => claim.Type == ClaimTypes.NameIdentifier).And
                .ContainSingle(claim => claim.Type == "batarangs").And
                .ContainSingle(claim => claim.Type == "money").And
                .ContainSingle(claim => claim.Type == "fight").And
                .ContainSingle(claim => claim.Type == ClaimTypes.Name).And
                .ContainSingle(claim => claim.Type == CustomClaimTypes.AccountId).And
                .ContainSingle(claim => claim.Type == ClaimTypes.Email)
            ;

            {
                Claim nameIdentifierClaim = jwtAccessToken.Claims.Single(claim => claim.Type == ClaimTypes.NameIdentifier);
                nameIdentifierClaim.Value.Should()
                    .Be(account.UserName);

                Claim nameClaim = jwtAccessToken.Claims.Single(claim => claim.Type == ClaimTypes.Name);
                nameClaim.Value.Should()
                    .Be(account.Name);

                Claim accountIdClaim = jwtAccessToken.Claims.Single(claim => claim.Type == CustomClaimTypes.AccountId);
                accountIdClaim.Value.Should()
                    .Be(account.UUID.ToString());

                Claim emailClaim = jwtAccessToken.Claims.Single(claim => claim.Type == ClaimTypes.Email);
                emailClaim.Value.Should()
                    .Be(account.Email);

                Claim batarangsClaim = jwtAccessToken.Claims.Single(claim => claim.Type == "batarangs");
                batarangsClaim.Value.Should()
                    .Be("10");

                Claim moneyClaim = jwtAccessToken.Claims.Single(claim => claim.Type == "money");
                moneyClaim.Value.Should()
                    .Be("1000 K€");

                Claim fightClaim = jwtAccessToken.Claims.Single(claim => claim.Type == "fight");
                fightClaim.Value.Should()
                    .Be("100");
            }
            SecurityToken refreshToken = authenticationToken.RefreshToken;

            refreshToken.Should()
                .NotBeNull();
            refreshToken.Id.Should()
                .NotBeNullOrWhiteSpace().And
                .MatchRegex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", "id must be a GUID");
            refreshToken.ValidFrom.Should()
                .Be(utcNow);
            refreshToken.ValidTo.Should()
                .Be(utcNow.AddMinutes(jwtInfos.RefreshTokenLifetime));
            refreshToken.Issuer.Should()
                .Be(jwtInfos.Issuer);
            JwtSecurityToken jwtRefreshToken = refreshToken.Should()
                .BeOfType<JwtSecurityToken>().Which;

            jwtRefreshToken.Audiences.Should()
                .HaveSameCount(jwtInfos.Audiences).And
                .OnlyHaveUniqueItems().And
                .BeEquivalentTo(jwtInfos.Audiences);

            jwtRefreshToken.Claims.Should()
                .ContainSingle(claim => claim.Type == ClaimTypes.NameIdentifier).And
                .ContainSingle(claim => claim.Type == ClaimTypes.Name).And
                .ContainSingle(claim => claim.Type == CustomClaimTypes.AccountId).And
                .ContainSingle(claim => claim.Type == CustomClaimTypes.Location).And
                .ContainSingle(claim => claim.Type == ClaimTypes.Email);

            {
                Claim nameIdentifierClaim = jwtRefreshToken.Claims.Single(claim => claim.Type == ClaimTypes.NameIdentifier);
                nameIdentifierClaim.Value.Should()
                    .Be(account.UserName);

                Claim nameClaim = jwtRefreshToken.Claims.Single(claim => claim.Type == ClaimTypes.Name);
                nameClaim.Value.Should()
                    .Be(account.Name);


                Claim accountIdClaim = jwtRefreshToken.Claims.Single(claim => claim.Type == CustomClaimTypes.AccountId);
                accountIdClaim.Value.Should()
                    .Be(account.UUID.ToString());

                Claim emailClaim = jwtRefreshToken.Claims.Single(claim => claim.Type == ClaimTypes.Email);
                emailClaim.Value.Should()
                    .Be(account.Email);
                Claim locationClaim = jwtRefreshToken.Claims.Single(claim => claim.Type == CustomClaimTypes.Location);
                locationClaim.Value.Should()
                    .Be(authenticationInfo.Location);
            }

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                string refreshTokenSaved = await uow.Repository<Account>()
                    .SingleAsync(x => x.RefreshToken, x => x.UUID == account.UUID)
                    .ConfigureAwait(false);

                refreshTokenSaved.Should()
                    .NotBeNullOrWhiteSpace("Refresh token must be saved in datastore").And
                    .Match(currentToken => currentToken.Like("*.*.*"), "Refresh token must match <header>.<payload>.<signature> pattern");
            }
        }


        [Fact]
        public async Task TwoTokenForSameAccount_Have_Differents_Ids()
        {
            // Arrange
            DateTime utcNow = 10.January(2014).AsUtc();
            Account account = new Account
            {
                UUID = Guid.NewGuid(),
                UserName = "thebatman",
                Email = "bwayne@wayne-enterprise.com",
                Name = "Bruce Wayne",
                PasswordHash = new Faker().Lorem.Word(),
                Salt = new Faker().Lorem.Word()
            };

            account.AddOrUpdateClaim("batarangs", "10", utcNow);
            account.AddOrUpdateClaim("fight", "100", utcNow);
            account.AddOrUpdateClaim("money", "1000 K€", utcNow);

            AccountInfo accountInfo;
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Create(account);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);

                IMapper mapper = AutoMapperConfig.Build().CreateMapper();
                accountInfo = mapper
                    .Map<Account, AccountInfo>(account);
            }

            JwtInfos jwtInfos = new JwtInfos
            {
                Issuer = "http://localhost:10000",
                Audiences = new[] { "api1", "api2" },
                Key = "key_to_encrypt_token",
                AccessTokenLifetime = 10,
                RefreshTokenLifetime = 1.Days().TotalMinutes
            };
            AuthenticationInfo authenticationInfo = new AuthenticationInfo { Location = "127.0.0.1" };

            _handleCreateSecurityTokenCommandMock.Setup(mock => mock.Handle(It.IsAny<CreateSecurityTokenCommand>(), It.IsAny<CancellationToken>()))
                .Returns(async (CreateSecurityTokenCommand request, CancellationToken ct) =>
                {
                    (JwtSecurityTokenOptions tokenOptions, IEnumerable<ClaimInfo> claims) = request.Data;
                    SecurityKey signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtInfos.Key));
                    SecurityToken token = new JwtSecurityToken(
                        issuer: tokenOptions.Issuer,
                        claims: claims.Select(claim => new Claim(claim.Type, claim.Value)),
                        notBefore: utcNow,
                        expires: utcNow.AddMinutes(tokenOptions.LifetimeInMinutes),
                        signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                    );

                    return await new ValueTask<SecurityToken>(token);
                });

            CreateAuthenticationTokenCommand cmd = new CreateAuthenticationTokenCommand((authenticationInfo, accountInfo, jwtInfos));
            _dateTimeServiceMock.Setup(mock => mock.UtcNow()).Returns(utcNow);

            // Act
            AuthenticationTokenInfo tokenOne = await _sut.Handle(cmd, ct: default)
                .ConfigureAwait(false);

            AuthenticationTokenInfo tokenTwo = await _sut.Handle(cmd, ct: default)
                .ConfigureAwait(false);

            // Assert
            tokenOne.AccessToken.Id.Should()
                .NotBe(tokenTwo.AccessToken.Id);

            tokenOne.RefreshToken.Id.Should()
                .NotBe(tokenTwo.RefreshToken.Id);
        }
    }
}
