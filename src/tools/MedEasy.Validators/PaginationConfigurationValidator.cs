using FluentValidation;
using MedEasy.RestObjects;
using static FluentValidation.Severity;

namespace MedEasy.Validators
{
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
                .GreaterThan(0)
                .WithSeverity(Error);
            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithSeverity(Error);
        }
    }
}
