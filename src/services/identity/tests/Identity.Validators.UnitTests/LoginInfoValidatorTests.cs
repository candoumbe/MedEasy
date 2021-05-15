namespace Identity.Validators.UnitTests
{
    using FluentAssertions;

    using FluentValidation;

    using Identity.DTO;

    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    [Feature("Validators")]
    public class LoginInfoValidatorTests
    {
        [Fact]
        public void IsLoginValidator() => typeof(LoginInfoValidator).Should()
            .Implement<IValidator<LoginInfo>>();
    }

}
