namespace MedEasy.Validators
{
    using FluentValidation;

    using MedEasy.Ids;

    using System;

    public sealed class StronglyTypedIdValidator : AbstractValidator<StronglyTypedId<Guid>>
    {
        public StronglyTypedIdValidator()
        {
            RuleFor(x => x.Value)
                   .NotEmpty();
        }
    }
}
