namespace Identity.Validators
{
    using FluentValidation;

    using Microsoft.IdentityModel.Tokens;

    using NodaTime;

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
