using FluentAssertions;
using FluentAssertions.Extensions;

using FluentValidation;
using FluentValidation.Results;

using Microsoft.IdentityModel.Tokens;

using Moq;

using NodaTime;
using NodaTime.Extensions;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

using static Moq.MockBehavior;

namespace Identity.Validators.UnitTests
{
    [UnitTest]
    [Feature("Validation")]
    [Feature("Authentication")]
    public class SecurityTokenLifetimeValidatorTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IClock> _datetimeServiceMock;


        public SecurityTokenLifetimeValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _datetimeServiceMock = new Mock<IClock>(Strict);
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
                    15.February(2014).Add(14.Hours().And(15.Minutes())).AsUtc().ToInstant(),
                    new JwtSecurityToken(
                        notBefore : 13.February(2014).Add(14.Hours().And(15.Minutes())).AsUtc(),
                        expires : 13.February(2014).Add(14.Hours().And(20.Minutes())).AsUtc()
                    ),
                    false,
                    "Current datetime is after token's lifetime"
                };

                yield return new object[]
                {
                    15.February(2014).Add(14.Hours().And(15.Minutes())).AsUtc().ToInstant(),
                    new JwtSecurityToken(
                        notBefore : 16.February(2014).Add(14.Hours().And(15.Minutes())).AsUtc(),
                        expires : 16.February(2014).Add(14.Hours().And(20.Minutes())).AsUtc()
                    ),
                    false,
                    "Current datetime is before token's lifetime"
                };

                yield return new object[]
                {
                    15.February(2014).Add(14.Hours().And(15.Minutes())).AsUtc().ToInstant(),
                    new JwtSecurityToken(
                        notBefore : 14.February(2014).Add(14.Hours().And(15.Minutes())).AsUtc(),
                        expires : 16.February(2014).Add(14.Hours().And(20.Minutes())).AsUtc()
                    ),
                    true,
                    "Current datetime is within token's lifetime"
                };

                yield return new object[]
                {
                    15.February(2014).Add(14.Hours().And(15.Minutes())).AsUtc().ToInstant(),
                    new JwtSecurityToken(
                        expires : 16.February(2014).Add(14.Hours().And(20.Minutes())).AsUtc()
                    ),
                    true,
                    "Current datetime is before token's expires"
                };

                yield return new object[]
                {
                    15.February(2014).Add(14.Hours().And(15.Minutes())).AsUtc().ToInstant(),
                    new JwtSecurityToken(
                        notBefore : 12.February(2014).Add(14.Hours().And(20.Minutes())).AsUtc()
                    ),
                    true,
                    "Current datetime is after token starts to be valid"
                };
            }
        }


        [Theory]
        [MemberData(nameof(ValidateCases))]
        public void Validate(Instant currentDate, SecurityToken securityToken, bool expectedValidity, string reason)
        {
            // Arrange
            _outputHelper.WriteLine($"Token valid from <{securityToken.ValidFrom}> to <{securityToken.ValidTo}>");
            _datetimeServiceMock.Setup(mock => mock.GetCurrentInstant()).Returns(currentDate);

            // Act
            ValidationResult validationResult = new SecurityTokenLifetimeValidator(_datetimeServiceMock.Object).Validate(securityToken);

            // Assert
            _datetimeServiceMock.Verify(mock => mock.GetCurrentInstant(), Times.Once);
            validationResult
                .IsValid
                .Should()
                .Be(expectedValidity, reason);
        }

        [Fact]
        public void IsValidator() => typeof(SecurityTokenLifetimeValidator).Should()
            .BeDerivedFrom<AbstractValidator<SecurityToken>>();
    }
}
