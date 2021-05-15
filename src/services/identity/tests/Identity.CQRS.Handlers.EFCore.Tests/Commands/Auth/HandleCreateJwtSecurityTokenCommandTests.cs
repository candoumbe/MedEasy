namespace Identity.CQRS.UnitTests.Handlers.Queries
{
    using FluentAssertions;
    using FluentAssertions.Extensions;

    using Identity.CQRS.Commands;
    using Identity.CQRS.Handlers;
    using Identity.DTO;
    using Identity.Ids;

    using MediatR;

    using Microsoft.IdentityModel.Tokens;

    using Moq;

    using NodaTime;
    using NodaTime.Extensions;

    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    using static Moq.MockBehavior;

    [UnitTest]
    [Feature("Identity")]
    [Feature("JWT")]
    public class HandleCreateJwtSecurityTokenCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IClock> _dateTimeServiceMock;
        private HandleCreateJwtSecurityTokenCommand _sut;

        public HandleCreateJwtSecurityTokenCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _dateTimeServiceMock = new Mock<IClock>(Strict);
            _sut = new HandleCreateJwtSecurityTokenCommand(dateTimeService: _dateTimeServiceMock.Object);
        }

        public void Dispose()
        {
            _outputHelper = null;
            _sut = null;
            _dateTimeServiceMock = null;
        }

        [Fact]
        public void Type_Is_A_Handler() =>
            // Assert
            _sut.GetType().Should()
                .Implement<IRequestHandler<CreateSecurityTokenCommand, SecurityToken>>();

        [Fact]
        public async Task GivenClaims_Handler_Returns_CorrespondingToken()
        {
            // Arrange

            Instant utcNow = 1.February(2007).Add(14.Hours()).AsUtc().ToInstant();
            JwtSecurityTokenOptions jwtSecurityTokenOptions = new()
            {
                Issuer = "http://localhost:10000",
                Audiences = new[] { "api1", "api2" },
                Key = "key_to_encrypt_token",
                LifetimeInMinutes = 1.Days().TotalMinutes
            };

            IEnumerable<ClaimInfo> claims = Enumerable.Empty<ClaimInfo>();

            CreateSecurityTokenCommand createRefreshTokenCommand = new((jwtSecurityTokenOptions, claims));
            _dateTimeServiceMock.Setup(mock => mock.GetCurrentInstant()).Returns(utcNow);

            // Act
            SecurityToken token = await _sut.Handle(createRefreshTokenCommand, ct: default)
                .ConfigureAwait(false);

            // Assert
            _dateTimeServiceMock.Verify();
            JwtSecurityToken jwtSecurityToken = token.Should()
                .BeAssignableTo<JwtSecurityToken>().Which;

            token.Should()
                .NotBeNull();
            token.Id.Should()
                .NotBeNullOrWhiteSpace().And
                .MatchRegex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$");

            jwtSecurityToken.Issuer.Should()
                .Be(jwtSecurityTokenOptions.Issuer);
            jwtSecurityToken.Audiences.Should()
                .HaveSameCount(jwtSecurityTokenOptions.Audiences).And
                .OnlyContain(audience => jwtSecurityTokenOptions.Audiences.Contains(audience));
            jwtSecurityToken.ValidFrom.Should()
                                      .Be(utcNow.ToDateTimeUtc());
            jwtSecurityToken.ValidTo.Should()
                .Be(utcNow.Plus(Duration.FromMinutes(jwtSecurityTokenOptions.LifetimeInMinutes)).ToDateTimeUtc());
            jwtSecurityToken.Claims.Should()
                .NotContainNulls().And
                .NotContain(claim => claim.Value == null);
        }

        [Fact]
        public async Task TwoTokenForSameAccount_Have_Differents_Ids()
        {
            // Arrange
            AccountInfo accountInfo = new()
            {
                Id = AccountId.New(),
                Username = "thebatman",
                Email = "bwayne@wayne-enterprise.com",
                Name = "Bruce Wayne",
                Claims = new[]
                {
                    new ClaimInfo { Type =  "batarangs", Value = "10"},
                    new ClaimInfo { Type =  "fight", Value = "100"},
                    new ClaimInfo { Type =  "money", Value = "1000K$"},

                }
            };

            JwtSecurityTokenOptions jwtInfos = new()
            {
                Issuer = "http://localhost:10000",
                Audiences = new[] { "api1", "api2" },
                Key = "key_to_encrypt_token",
                LifetimeInMinutes = 10
            };

            Instant utcNow = 10.January(2014).AsUtc().ToInstant();
            AuthenticationInfo authInfo = new() { Location = "Paris" };
            CreateSecurityTokenCommand createRefreshTokenCommand = new((jwtInfos, Enumerable.Empty<ClaimInfo>()));
            _dateTimeServiceMock.Setup(mock => mock.GetCurrentInstant()).Returns(utcNow);

            // Act
            SecurityToken tokenOne = await _sut.Handle(createRefreshTokenCommand, ct: default)
                .ConfigureAwait(false);

            SecurityToken tokenTwo = await _sut.Handle(createRefreshTokenCommand, ct: default)
                .ConfigureAwait(false);

            // Assert
            tokenOne.Id.Should()
                .NotBeNullOrWhiteSpace().And
                .NotBe(tokenTwo.Id);
        }
    }
}
