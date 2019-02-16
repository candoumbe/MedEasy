using FluentValidation;
using Measures.DTO;
using MedEasy.Validators.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using static FluentValidation.Severity;

namespace Measures.Validators.Queries.BloodPressures
{
    /// <summary>
    /// Validates <see cref="SearchBloodPressureInfo"/> instances.
    /// </summary>
    public class SearchBloodPressureInfoValidator : AbstractValidator<SearchBloodPressureInfo>
    {
        /// <summary>
        /// Builds a new <see cref="SearchBloodPressureInfoValidator"/> instance.
        /// </summary>
        public SearchBloodPressureInfoValidator()
        {
            Include(new AbstractSearchInfoValidator<BloodPressureInfo>());


            RuleFor(x => x.From)
                .NotNull()
                .Unless(x => x.To != default || x.Sort != default || x.PatientId != default);

            RuleFor(x => x.To)
                .NotNull()
                .Unless(x => x.From != default || x.Sort != default || x.PatientId != default);

            RuleFor(x => x.Sort)
                .NotEmpty()
                .Unless(x => x.From.HasValue || x.To.HasValue || x.PatientId != default);

            When(
                (x) => x.From.HasValue && x.To.HasValue,
                () =>
                {
                    RuleFor(x => x.From)
                        .LessThanOrEqualTo(x => x.To);
                }
            );
        }
    }
}
