using FluentValidation;

using Microsoft.IdentityModel.Tokens;

using NodaTime;

namespace Identity.Validators
{
    public class SecurityTokenLifetimeValidator : AbstractValidator<SecurityToken>
    {
        public SecurityTokenLifetimeValidator(IClock datetimeService)
        {
            Instant utcNow = datetimeService.GetCurrentInstant();
            RuleFor(x => x.ValidFrom)
                .LessThanOrEqualTo(utcNow.ToDateTimeUtc())
                .Unless(token => token.ValidFrom == default);

            RuleFor(x => x.ValidTo)
                .GreaterThanOrEqualTo(utcNow.ToDateTimeUtc())
                .Unless(token => token.ValidTo == default);

        }
    }
}
