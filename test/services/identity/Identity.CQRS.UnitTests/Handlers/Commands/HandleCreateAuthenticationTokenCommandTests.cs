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
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Moq.MockBehavior;

namespace Identity.CQRS.UnitTests.Handlers.Queries
{
    [UnitTest]
    [Feature("Identity")]
    public class HandleCreateAuthenticationTokenCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IDateTimeService> _dateTimeServiceMock;
        private HandleCreateAuthenticationTokenCommand _sut;

        public HandleCreateAuthenticationTokenCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _dateTimeServiceMock = new Mock<IDateTimeService>(Strict);
            _sut = new HandleCreateAuthenticationTokenCommand(dateTimeService: _dateTimeServiceMock.Object);
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
                .Implement<IRequestHandler<CreateAuthenticationTokenCommand, SecurityToken>>();


        [Fact]
        public async Task GivenAccountInfo_Handler_Returns_CorrespondingToken()
        {
            // Arrange
            AccountInfo accountInfo = new AccountInfo
            {
                Id = Guid.NewGuid(),
                Username = "thebatman",
                Email = "bwayne@wayne-enterprise.com",
                Firstname = "Bruce",
                Lastname = "Wayne",
                Claims = new[]
                {
                    new ClaimInfo { Type =  "batarangs", Value = "10"},
                    new ClaimInfo { Type =  "fight", Value = "100"},
                    new ClaimInfo { Type =  "money", Value = "1000K$"},

                }
            };
            JwtInfos jwtInfos = new JwtInfos
            {
                Issuer = "http://localhost:10000",
                Audiences = new []{ "api1", "api2" },
                Key = "key_to_encrypt_token",
                Validity = 10
            };
            DateTimeOffset utcNow = 10.January(2014);
            CreateAuthenticationTokenCommand cmd = new CreateAuthenticationTokenCommand((accountInfo, jwtInfos));
            _dateTimeServiceMock.Setup(mock => mock.UtcNowOffset()).Returns(utcNow);


            // Act
            SecurityToken token = await _sut.Handle(cmd, ct: default)
                .ConfigureAwait(false);

            // Assert
            _dateTimeServiceMock.Verify();
            token.Should()
                .NotBeNull();
            token.Id.Should()
                .NotBeNullOrWhiteSpace().And
                .MatchRegex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", "id must be a GUID");
            token.ValidFrom.Should()
                .Be(utcNow.UtcDateTime);
            token.ValidTo.Should()
                .Be(utcNow.AddMinutes(jwtInfos.Validity).UtcDateTime);
            token.Issuer.Should()
                .Be(jwtInfos.Issuer);

            JwtSecurityToken jwtToken = token.Should()
                .BeOfType<JwtSecurityToken>().Which;

            jwtToken.Audiences.Should()
                .BeEquivalentTo(jwtInfos.Audiences);

            jwtToken.Claims.Should()
                .Contain(claim => claim.Type == "batarangs").And
                .Contain(claim => claim.Type == "money").And
                .Contain(claim => claim.Type == "fight")
                ;

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
                Firstname = "Bruce",
                Lastname = "Wayne",
                Claims = new[]
                {
                    new ClaimInfo { Type =  "batarangs", Value = "10"},
                    new ClaimInfo { Type =  "fight", Value = "100"},
                    new ClaimInfo { Type =  "money", Value = "1000K$"},

                }
            };
            JwtInfos jwtInfos = new JwtInfos
            {
                Issuer = "http://localhost:10000",
                Audiences = new[] { "api1", "api2" },
                Key = "key_to_encrypt_token",
                Validity = 10
            };
            DateTimeOffset utcNow = 10.January(2014);
            CreateAuthenticationTokenCommand cmd = new CreateAuthenticationTokenCommand((accountInfo, jwtInfos));
            _dateTimeServiceMock.Setup(mock => mock.UtcNowOffset()).Returns(utcNow);


            // Act
            SecurityToken tokenOne = await _sut.Handle(cmd, ct: default)
                .ConfigureAwait(false);

            SecurityToken tokenTwo= await _sut.Handle(cmd, ct: default)
                .ConfigureAwait(false);

            // Assert
            tokenOne.Id.Should()
                .NotBe(tokenTwo.Id);

        }

    }
}
