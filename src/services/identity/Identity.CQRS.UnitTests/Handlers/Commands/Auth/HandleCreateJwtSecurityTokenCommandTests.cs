using FluentAssertions;
using FluentAssertions.Extensions;
using Identity.CQRS.Commands;
using Identity.CQRS.Handlers.Commands;
using Identity.DTO;
using MedEasy.Abstractions;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Moq.MockBehavior;

namespace Identity.CQRS.UnitTests.Handlers.Queries
{
    [UnitTest]
    [Feature("Identity")]
    [Feature("JWT")]
    public class HandleCreateJwtSecurityTokenCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IDateTimeService> _dateTimeServiceMock;
        private HandleCreateJwtSecurityTokenCommand _sut;

        public HandleCreateJwtSecurityTokenCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _dateTimeServiceMock = new Mock<IDateTimeService>(Strict);
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

            DateTime utcNow = 1.February(2007).Add(14.Hours()).AsUtc();
            JwtSecurityTokenOptions jwtSecurityTokenOptions = new JwtSecurityTokenOptions
            {
                Issuer = "http://localhost:10000",
                Audiences = new[] { "api1", "api2" },
                Key = "key_to_encrypt_token",
                LifetimeInMinutes = 1.Days().TotalMinutes
            };

            IEnumerable<ClaimInfo> claims = Enumerable.Empty<ClaimInfo>();

            CreateSecurityTokenCommand createRefreshTokenCommand = new CreateSecurityTokenCommand((jwtSecurityTokenOptions, claims));
            _dateTimeServiceMock.Setup(mock => mock.UtcNow()).Returns(utcNow);

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
                .Be(utcNow);
            jwtSecurityToken.ValidTo.Should()
                .Be(utcNow.AddMinutes(jwtSecurityTokenOptions.LifetimeInMinutes));
            jwtSecurityToken.Claims.Should()
                .NotContainNulls().And
                .NotContain(claim => claim.Value == null);
        }

        [Fact]
        public async Task TwoTokenForSameAccount_Have_Differents_Ids()
        {
            // Arrange
            AccountInfo accountInfo = new AccountInfo
            {
                Id = Guid.NewGuid(),
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

            JwtSecurityTokenOptions jwtInfos = new JwtSecurityTokenOptions
            {
                Issuer = "http://localhost:10000",
                Audiences = new[] { "api1", "api2" },
                Key = "key_to_encrypt_token",
                LifetimeInMinutes = 10
            };

            DateTime utcNow = 10.January(2014).AsUtc();
            AuthenticationInfo authInfo = new AuthenticationInfo { Location = "Paris" };
            CreateSecurityTokenCommand createRefreshTokenCommand = new CreateSecurityTokenCommand((jwtInfos, Enumerable.Empty<ClaimInfo>()));
            _dateTimeServiceMock.Setup(mock => mock.UtcNow()).Returns(utcNow);

            // Act
            SecurityToken tokenOne = await _sut.Handle(createRefreshTokenCommand, ct: default)
                .ConfigureAwait(false);

            SecurityToken tokenTwo= await _sut.Handle(createRefreshTokenCommand, ct: default)
                .ConfigureAwait(false);

            // Assert
            tokenOne.Id.Should()
                .NotBeNullOrWhiteSpace().And
                .NotBe(tokenTwo.Id);
        }
    }
}
