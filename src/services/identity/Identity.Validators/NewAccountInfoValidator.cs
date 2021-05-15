namespace Identity.Validators
{
    using FluentValidation;

    using Identity.DTO;

    public class NewAccountInfoValidator : AbstractValidator<NewAccountInfo>
    {
        public NewAccountInfoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotNull();

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .Equal(x => x.Password);

            RuleFor(x => x.Username)
                .NotEmpty();
        }
    }
}
