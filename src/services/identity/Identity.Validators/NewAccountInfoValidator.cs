namespace Identity.Validators
{
    using FluentValidation;

    using Identity.DTO;
    using MedEasy.ValueObjects;

    public class NewAccountInfoValidator : AbstractValidator<NewAccountInfo>
    {
        public NewAccountInfoValidator()
        {
            RuleFor(x => x.Email)
                .NotNull()
                .NotEqual(Email.Empty);

            RuleFor(x => x.Password)
                .NotNull();

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .Equal(x => x.Password);

            RuleFor(x => x.Username)
                .NotNull()
                .NotEqual(UserName.Empty);
        }
    }
}
