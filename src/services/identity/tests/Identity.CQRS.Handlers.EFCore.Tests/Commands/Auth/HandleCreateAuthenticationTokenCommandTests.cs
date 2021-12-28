namespace Identity.CQRS.UnitTests.Handlers.Queries
{
    using AutoMapper;

    using Bogus;

    using FluentAssertions;
    using FluentAssertions.Extensions;

    using Identity.CQRS.Commands;
    using Identity.CQRS.Handlers;
    using Identity.CQRS.Handlers.EFCore.Commands.Auth;
    using Identity.DataStores;
    using Identity.DTO;
    using Identity.Ids;
    using Identity.Mapping;
    using Identity.Objects;

    using MedEasy.DAL.EFStore;
    using MedEasy.DAL.Interfaces;
    using MedEasy.IntegrationTests.Core;

    using MediatR;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;

    using Moq;

    using NodaTime;
    using NodaTime.Extensions;
    using NodaTime.Testing;

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

    [UnitTest]
    [Feature("Identity")]
    [Feature("JWT")]
    [Feature("Authentication")]
    public class HandleCreateAuthenticationTokenCommandTests : IAsyncLifetime, IClassFixture<SqliteEfCoreDatabaseFixture<IdentityContext>>
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IClock> _dateTimeServiceMock;
        private HandleCreateAuthenticationTokenCommand _sut;
        private IUnitOfWorkFactory _uowFactory;
        private Mock<IHandleCreateSecurityTokenCommand> _handleCreateSecurityTokenCommandMock;

        public HandleCreateAuthenticationTokenCommandTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<IdentityContext> databaseFixture)
        {
            _outputHelper = outputHelper;

            _dateTimeServiceMock = new Mock<IClock>(Strict);

            _uowFactory = new EFUnitOfWorkFactory<IdentityContext>(databaseFixture.OptionsBuilder.Options,
                                                                   (options) =>
                                                                   {
                                                                       IdentityContext context = new(options, new FakeClock(new Instant()));
                                                                       context.Database.EnsureCreated();
                                                                       return context;
                                                                   });
            _handleCreateSecurityTokenCommandMock = new Mock<IHandleCreateSecurityTokenCommand>(Strict);

            _sut = new HandleCreateAuthenticationTokenCommand(dateTimeService: _dateTimeServiceMock.Object, unitOfWorkFactory: _uowFactory, _handleCreateSecurityTokenCommandMock.Object);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            _outputHelper = null;
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();

            uow.Repository<Account>().Clear();
            uow.Repository<Role>().Clear();
            await uow.SaveChangesAsync()
                     .ConfigureAwait(false);
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
            Instant utcNow = 10.January(2014).AsUtc().ToInstant();
            Account account = new(id: AccountId.New(),
                                  username: "thebatman",
                                  email: "bwayne@wayne-enterprise.com",
                                  name: "Bruce Wayne",
                                  passwordHash: new Faker().Lorem.Word(),
                                  salt: new Faker().Lorem.Word());

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

            JwtInfos jwtInfos = new()
            {
                Issuer = "http://localhost:10000",
                Audiences = new[] { "api1", "api2" },
                Key = "key_to_encrypt_token",
                AccessTokenLifetime = 10,
                RefreshTokenLifetime = 1.Days().TotalMinutes
            };
            _dateTimeServiceMock.Setup(mock => mock.GetCurrentInstant()).Returns(utcNow);

            _handleCreateSecurityTokenCommandMock.Setup(mock => mock.Handle(It.IsAny<CreateSecurityTokenCommand>(), It.IsAny<CancellationToken>()))
                .Returns(async (CreateSecurityTokenCommand cmd, CancellationToken _) =>
                {
                    (JwtSecurityTokenOptions tokenOptions, Instant utcNow, IEnumerable<ClaimInfo> claims) = cmd.Data;
                    SecurityKey signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtInfos.Key));
                    SecurityToken token = new JwtSecurityToken(
                        issuer: tokenOptions.Issuer,
                        claims: claims.Select(claim => new Claim(claim.Type, claim.Value)),
                        notBefore: utcNow.ToDateTimeUtc(),
                        expires: utcNow.Plus(Duration.FromMinutes(tokenOptions.LifetimeInMinutes)).ToDateTimeUtc(),
                        signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                    );

                    return await new ValueTask<SecurityToken>(token);
                });

            AuthenticationInfo authenticationInfo = new() { Location = "127.0.0.1" };
            CreateAuthenticationTokenCommand createAuthenticationTokenCommand = new((authenticationInfo, accountInfo, jwtInfos));

            // Act
            AuthenticationTokenInfo authenticationToken = await _sut.Handle(createAuthenticationTokenCommand, ct: default)
                                                                    .ConfigureAwait(false);

            // Assert
            _dateTimeServiceMock.Verify();
            _handleCreateSecurityTokenCommandMock.Verify(mock => mock.Handle(It.IsAny<CreateSecurityTokenCommand>(),
                                                                             It.IsAny<CancellationToken>()), Times.Exactly(2));

            SecurityToken accessToken = authenticationToken.AccessToken;

            accessToken.Should()
                .NotBeNull();
            accessToken.Id.Should()
                .NotBeNullOrWhiteSpace().And
                .MatchRegex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", "id must be a GUID");
            accessToken.ValidFrom.Should()
                .Be(utcNow.ToDateTimeUtc());
            accessToken.ValidTo.Should()
                .Be(utcNow.Plus(Duration.FromMinutes(jwtInfos.AccessTokenLifetime)).ToDateTimeUtc());
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
                                .ContainSingle(claim => claim.Type == CustomClaimTypes.TimeZoneId).And
                                .ContainSingle(claim => claim.Type == ClaimTypes.Email)
            ;

            {
                Claim nameIdentifierClaim = jwtAccessToken.Claims.Single(claim => claim.Type == ClaimTypes.NameIdentifier);
                nameIdentifierClaim.Value.Should()
                    .Be(account.Username);

                Claim nameClaim = jwtAccessToken.Claims.Single(claim => claim.Type == ClaimTypes.Name);
                nameClaim.Value.Should()
                    .Be(account.Name);

                Claim accountIdClaim = jwtAccessToken.Claims.Single(claim => claim.Type == CustomClaimTypes.AccountId);
                accountIdClaim.Value.Should()
                    .Be(account.Id.ToString());

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

                Claim timeZoneClaim = jwtAccessToken.Claims.Single(claim => claim.Type == CustomClaimTypes.TimeZoneId);
                timeZoneClaim.Value.Should()
                                   .Be(DateTimeZone.Utc.Id, $"{nameof(CustomClaimTypes.TimeZoneId)} must be set even when account has not explicitely specified");
            }
            SecurityToken refreshToken = authenticationToken.RefreshToken;

            refreshToken.Should()
                .NotBeNull();
            refreshToken.Id.Should()
                .NotBeNullOrWhiteSpace().And
                .MatchRegex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", "id must be a GUID");
            refreshToken.ValidFrom.Should()
                                  .Be(utcNow.ToDateTimeUtc());
            refreshToken.ValidTo.Should()
                .Be(utcNow.Plus(Duration.FromMinutes(jwtInfos.RefreshTokenLifetime)).ToDateTimeUtc());
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
                    .Be(account.Username);

                Claim nameClaim = jwtRefreshToken.Claims.Single(claim => claim.Type == ClaimTypes.Name);
                nameClaim.Value.Should()
                    .Be(account.Name);


                Claim accountIdClaim = jwtRefreshToken.Claims.Single(claim => claim.Type == CustomClaimTypes.AccountId);
                accountIdClaim.Value.Should()
                    .Be(account.Id.ToString());

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
                    .SingleAsync(x => x.RefreshToken, x => x.Id == account.Id)
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
            Instant utcNow = 10.January(2014).AsUtc().ToInstant();
            Faker faker = new();
            Account account = new(id: AccountId.New(),
                                   username: "thebatman",
                                   email: "bwayne@wayne-enterprise.com",
                                   name: "Bruce Wayne",
                                   passwordHash: faker.Lorem.Word(),
                                   salt: faker.Lorem.Word());

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

            JwtInfos jwtInfos = new()
            {
                Issuer = "http://localhost:10000",
                Audiences = new[] { "api1", "api2" },
                Key = "key_to_encrypt_token",
                AccessTokenLifetime = 10,
                RefreshTokenLifetime = 1.Days().TotalMinutes
            };
            AuthenticationInfo authenticationInfo = new() { Location = "127.0.0.1" };

            _handleCreateSecurityTokenCommandMock.Setup(mock => mock.Handle(It.IsAny<CreateSecurityTokenCommand>(), It.IsAny<CancellationToken>()))
                .Returns(async (CreateSecurityTokenCommand request, CancellationToken _) =>
                {
                    (JwtSecurityTokenOptions tokenOptions, Instant utcNow, IEnumerable<ClaimInfo> claims) = request.Data;
                    SecurityKey signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtInfos.Key));
                    SecurityToken token = new JwtSecurityToken(
                        issuer: tokenOptions.Issuer,
                        claims: claims.Select(claim => new Claim(claim.Type, claim.Value)),
                        notBefore: utcNow.ToDateTimeUtc(),
                        expires: utcNow.Plus(Duration.FromMinutes(tokenOptions.LifetimeInMinutes)).ToDateTimeUtc(),
                        signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                    );

                    return await new ValueTask<SecurityToken>(token).ConfigureAwait(false);
                });

            CreateAuthenticationTokenCommand cmd = new((authenticationInfo, accountInfo, jwtInfos));
            _dateTimeServiceMock.Setup(mock => mock.GetCurrentInstant()).Returns(utcNow);

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
