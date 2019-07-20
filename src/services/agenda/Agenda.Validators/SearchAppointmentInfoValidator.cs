using Agenda.DTO;
using Agenda.DTO.Resources.Search;
using FluentValidation;
using MedEasy.Abstractions;
using MedEasy.Validators.Validators;
using System;

namespace Agenda.Validators
{
    /// <summary>
    /// Validates <see cref="SearchAppointmentInfo"/> instances.
    /// </summary>
    public class SearchAppointmentInfoValidator : AbstractValidator<SearchAppointmentInfo>
    {
        /// <summary>
        /// Builds a new <see cref="SearchAppointmentInfoValidator"/> instance
        /// </summary>
        public SearchAppointmentInfoValidator()
        {
            Include(new AbstractSearchInfoValidator<AppointmentInfo>());

            When(x => x.From.HasValue && x.To.HasValue,
                () =>
                {
                    RuleFor(x => x.To)
                        .GreaterThanOrEqualTo((search) => search.From);
                });
        }
    }
}
