using Agenda.DTO;

using FluentValidation;

using NodaTime;

using System;

namespace Agenda.Validators
{
    /// <summary>
    /// Validates <see cref="NewAppointmentInfo"/> instances.
    /// </summary>
    public class NewAppointmentModelValidator : AbstractValidator<NewAppointmentInfo>
    {
        /// <summary>
        /// Builds a new <see cref="NewAppointmentModelValidator"/> instance
        /// </summary>
        /// <param name="dateTimeService">Service to get <see cref="DateTime"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="dateTimeService"/> is null.</exception>
        public NewAppointmentModelValidator(IClock dateTimeService)
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

            RuleFor(x => x.Attendees)
                .NotEmpty();

            When(
                x => x.StartDate != default && x.EndDate != default,
                () =>
                {
                    RuleFor(x => x.EndDate)
                        .Must((x, endDate) => endDate.ToInstant() >= x.StartDate.ToInstant());
                }
            );
        }
    }
}
