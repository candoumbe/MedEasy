using Agenda.DTO;
using Agenda.DTO.Resources.Search;

using FluentValidation;

using MedEasy.Validators.Validators;

namespace Agenda.Validators
{
    /// <summary>
    /// Validates <see cref="SearchAttendeeInfo"/> instances.
    /// </summary>
    public class SearchParticipantInfoValidator : AbstractValidator<SearchAttendeeInfo>
    {
        /// <summary>
        /// Builds a new <see cref="SearchParticipantInfoValidator"/> instance
        /// </summary>
        public SearchParticipantInfoValidator()
        {
            Include(new AbstractSearchInfoValidator<AttendeeInfo>());
        }
    }
}
