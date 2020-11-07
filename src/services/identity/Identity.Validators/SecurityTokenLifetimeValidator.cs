using FluentValidation;

using MedEasy.Abstractions;

using Microsoft.IdentityModel.Tokens;

using System;

namespace Identity.Validators
{
    public class SecurityTokenLifetimeValidator : AbstractValidator<SecurityToken>
    {
        public SecurityTokenLifetimeValidator(IDateTimeService datetimeService)
        {
            DateTime utcNow = datetimeService.UtcNow();
            RuleFor(x => x.ValidFrom)
                .LessThanOrEqualTo(utcNow)
                .Unless(token => token.ValidFrom == default(DateTime));

            RuleFor(x => x.ValidTo)
                .GreaterThanOrEqualTo(utcNow)
                .Unless(token => token.ValidTo == default(DateTime));

        }
    }
}
