using FluentAssertions;

using FluentValidation;

using Identity.DTO;

using Xunit;
using Xunit.Categories;

namespace Identity.Validators.UnitTests
{

    [UnitTest]
    [Feature("Validators")]
    public class LoginInfoValidatorTests
    {
        [Fact]
        public void IsLoginValidator() => typeof(LoginInfoValidator).Should()
            .Implement<IValidator<LoginInfo>>();
    }

}
