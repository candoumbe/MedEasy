
using FluentAssertions;

using FluentValidation;

using MedEasy.Ids;

using System;

using Xunit;
using Xunit.Categories;

namespace MedEasy.Validators.Tests
{
    [UnitTest]
    public class StronglyTypedIdValidatorTests
    {

        [Fact]
        public void StronglyTypedIdValidator_is_a_validator_for_StronglyTypedId() => typeof(StronglyTypedIdValidator).Should()
                                                                                                                     .BeDerivedFrom<AbstractValidator<StronglyTypedId<Guid>>>().And
                                                                                                                     .BeSealed().And
                                                                                                                     .HaveDefaultConstructor();

    }
}
