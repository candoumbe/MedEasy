﻿using Agenda.DTO;
using FluentValidation;
using MedEasy.Abstractions;
using System;

namespace Agenda.Validators
{
    /// <summary>
    /// Validates <see cref="NewAppointmentInfo"/> instances.
    /// </summary>
    public class NewAppointmentInfoValidator : AbstractValidator<NewAppointmentInfo>
    {
        /// <summary>
        /// Builds a new <see cref="NewAppointmentInfoValidator"/> instance
        /// </summary>
        /// <param name="dateTimeService">Service to get <see cref="DateTime"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="dateTimeService"/> is null.</exception>
        public NewAppointmentInfoValidator(IDateTimeService dateTimeService)
        {
            if (dateTimeService == null)
            {
                throw new ArgumentNullException(nameof(dateTimeService));
            }

            RuleFor(x => x.EndDate)
                .NotEmpty();
            RuleFor(x => x.Location)
                .NotEmpty();
            RuleFor(x => x.StartDate)
                .NotEmpty();
            RuleFor(x => x.Subject)
                .NotNull();

            RuleFor(x => x.Participants)
                .NotEmpty();

            When(
                x => x.StartDate != default && x.EndDate != default,
                () =>
                {
                    RuleFor(x => x.EndDate)
                        .GreaterThanOrEqualTo(x => x.StartDate);
                }
            );

            
        }
    }
}