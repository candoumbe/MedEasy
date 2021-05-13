using FluentValidation;

using MedEasy.Ids;

using System;

namespace MedEasy.Validators
{

    public sealed class StronglyTypedIdValidator : AbstractValidator<StronglyTypedId<Guid>>
    {
        public StronglyTypedIdValidator()
        {
            RuleFor(x => x.Value)
                   .NotEmpty();
        }
    }
}
