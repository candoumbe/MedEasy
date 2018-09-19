using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Identity.DTO;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
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
