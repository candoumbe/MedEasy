using Agenda.DTO;
using Agenda.DTO.Resources.Search;
using FluentValidation;
using MedEasy.Abstractions;
using MedEasy.Validators.Validators;
using System;

namespace Agenda.Validators
{
    /// <summary>
    /// Validates <see cref="SearchParticipantInfo"/> instances.
    /// </summary>
    public class SearchParticipantInfoValidator : AbstractValidator<SearchParticipantInfo>
    {
        /// <summary>
        /// Builds a new <see cref="SearchParticipantInfoValidator"/> instance
        /// </summary>
        public SearchParticipantInfoValidator()
        {
            Include(new AbstractSearchInfoValidator<ParticipantInfo>());
        }
    }
}
