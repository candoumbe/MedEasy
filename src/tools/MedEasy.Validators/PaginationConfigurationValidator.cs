namespace MedEasy.Validators
{
    using FluentValidation;

    using MedEasy.RestObjects;

    using static FluentValidation.Severity;

    /// <summary>
    /// Validator for <see cref="PaginationConfiguration"/>
    /// </summary>
    public class PaginationConfigurationValidator : AbstractValidator<PaginationConfiguration>
    {
        /// <summary>
        /// Builds a new <see cref="PaginationConfigurationValidator"/> instance.
        /// </summary>
        public PaginationConfigurationValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1)
                .WithSeverity(Error);
            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1)
                .WithSeverity(Error);
        }
    }
}
