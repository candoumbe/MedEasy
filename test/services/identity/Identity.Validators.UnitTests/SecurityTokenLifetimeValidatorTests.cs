using MedEasy.Abstractions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static Moq.MockBehavior;
using FluentAssertions;
using FluentAssertions.Extensions;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Xunit.Categories;
using FluentValidation;
using FluentValidation.Results;
using System.Linq.Expressions;

namespace Identity.Validators.UnitTests
{
    [UnitTest]
    [Feature("Validation")]
    [Feature("Authentication")]
    public class SecurityTokenLifetimeValidatorTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IDateTimeService> _datetimeServiceMock;


        public SecurityTokenLifetimeValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _datetimeServiceMock = new Mock<IDateTimeService>(Strict);
        }

        public void Dispose()
        {
            _outputHelper = null;
            _datetimeServiceMock = null;
        }

        public static IEnumerable<object[]> ValidateCases
        {
            get
            {
                yield return new object[]
                {
                    15.February(2014).Add(14.Hours().Add(15.Minutes())),
                    new JwtSecurityToken(
                        notBefore : 13.February(2014).Add(14.Hours().Add(15.Minutes())),
                        expires : 13.February(2014).Add(14.Hours().Add(20.Minutes()))
                    ),
                    false,
                    "Current datetime is after token's lifetime"
                };

                yield return new object[]
                {
                    15.February(2014).Add(14.Hours().Add(15.Minutes())),
                    new JwtSecurityToken(
                        notBefore : 16.February(2014).Add(14.Hours().Add(15.Minutes())),
                        expires : 16.February(2014).Add(14.Hours().Add(20.Minutes()))
                    ),
                    false,
                    "Current datetime is before token's lifetime"
                };

                yield return new object[]
                {
                    15.February(2014).Add(14.Hours().Add(15.Minutes())),
                    new JwtSecurityToken(
                        notBefore : 14.February(2014).Add(14.Hours().Add(15.Minutes())),
                        expires : 16.February(2014).Add(14.Hours().Add(20.Minutes()))
                    ),
                    true,
                    "Current datetime is within token's lifetime"
                };

                yield return new object[]
                {
                    15.February(2014).Add(14.Hours().Add(15.Minutes())),
                    new JwtSecurityToken(
                        expires : 16.February(2014).Add(14.Hours().Add(20.Minutes()))
                    ),
                    true,
                    "Current datetime is before token's expires"
                };

                yield return new object[]
                {
                    15.February(2014).Add(14.Hours().Add(15.Minutes())),
                    new JwtSecurityToken(
                        notBefore : 12.February(2014).Add(14.Hours().Add(20.Minutes()))
                    ),
                    true,
                    "Current datetime is after token starts to be valid"
                };
            }
        }




        [Theory]
        [MemberData(nameof(ValidateCases))]
        public void Validate(DateTime currentDate, SecurityToken securityToken, bool expectedValidity, string reason)
        {

            // Arrange
            _outputHelper.WriteLine($"Token valid from <{securityToken.ValidFrom}> to <{securityToken.ValidTo}>");
            _datetimeServiceMock.Setup(mock => mock.UtcNow()).Returns(currentDate);

            // Act
            ValidationResult validationResult = new SecurityTokenLifetimeValidator(_datetimeServiceMock.Object).Validate(securityToken);

            // Assert
            _datetimeServiceMock.Verify(mock => mock.UtcNow(), Times.Once);
            validationResult
                .IsValid
                .Should()
                .Be(expectedValidity, reason);        }

        [Fact]
        public void IsValidator() => typeof(SecurityTokenLifetimeValidator).Should()
            .BeDerivedFrom<AbstractValidator<SecurityToken>>();
    }
}
