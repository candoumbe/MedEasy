using Bogus;

using FluentAssertions;

using FluentValidation;

using Identity.DTO;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

using ValidationResult = FluentValidation.Results.ValidationResult;

namespace Identity.Validators.UnitTests
{
    [UnitTest]
    [Feature("Accounts")]
    public class NewAccountInfoValidatorTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly NewAccountInfoValidator _sut;

        public NewAccountInfoValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _sut = new NewAccountInfoValidator();
        }

        [Fact]
        public void IsNewAccountValidator() => typeof(NewAccountInfoValidator).Should()
            .Implement<IValidator<NewAccountInfo>>();

        public static IEnumerable<object[]> ValidationCases
        {
            get
            {
                Faker faker = new();
                yield return new object[]
                {
                    new NewAccountInfo(),
                    (Expression<Func<ValidationResult, bool>>)(vr =>
                        vr.Errors.Count == 4
                        && vr.Errors.Once(err => err.PropertyName == nameof(NewAccountInfo.Email))
                        && vr.Errors.Once(err => err.PropertyName == nameof(NewAccountInfo.Password))
                        && vr.Errors.Once(err => err.PropertyName == nameof(NewAccountInfo.ConfirmPassword))
                        && vr.Errors.Once(err => err.PropertyName == nameof(NewAccountInfo.Username))

                    ),
                    "No property set"
                };

                yield return new object[]
                {
                    new NewAccountInfo
                    {
                        Name = faker.Person.Company.Name,
                        Email = "joker@card-city.com",
                        Password = "smile",
                        ConfirmPassword = "smiles",
                        Username = faker.Person.UserName
                    },
                    (Expression<Func<ValidationResult, bool>>)(vr =>
                        vr.Errors.Count == 1
                        && vr.Errors.Once(err => err.PropertyName == nameof(NewAccountInfo.ConfirmPassword))

                    ),
                    $"{nameof(NewAccountInfo.Password)} && {nameof(NewAccountInfo.ConfirmPassword)} don't match"
                };


                yield return new object[]
                {
                    new NewAccountInfo
                    {
                        Name = "The dark knight",
                        Email = "batman@gotham.fr",
                        Password = "smile",
                        ConfirmPassword = "smile",
                        Username = "capedcrusader"
                    },
                    (Expression<Func<ValidationResult, bool>>)(vr => vr.Errors.Count == 0),
                    "Informations are ok"
                };

            }
        }

        [Theory]
        [MemberData(nameof(ValidationCases))]
        public async Task ValidateTests(NewAccountInfo newAccountInfo, Expression<Func<ValidationResult, bool>> validationResultExpectation, string reason)
        {
            // Arrange
            _outputHelper.WriteLine($"NewAccount : {newAccountInfo.Jsonify()}");

            // Act
            ValidationResult vr = await _sut.ValidateAsync(newAccountInfo, default)
                .ConfigureAwait(false);

            // Assert
            _outputHelper.WriteLine($"Validation results : {vr.Jsonify()}");
            vr.Should()
                .Match(validationResultExpectation, reason);

        }
    }
}
