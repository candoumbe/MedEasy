namespace Agenda.CQRS.Features.Appointments.Commands
{
    using Agenda.Ids;

    using MedEasy.CQRS.Core.Commands;
    using MedEasy.CQRS.Core.Commands.Results;

    using NodaTime;

    using System;

    /// <summary>
    /// Command to change an <see cref="Objects.Appointment"/>'s <see cref="Objects.Appointment.StartDate"/>/<see cref="Objects.Appointment.EndDate"/>.
    /// </summary>
    public class ChangeAppointmentDateCommand : CommandBase<Guid, (AppointmentId appointmentId, ZonedDateTime start, ZonedDateTime end), ModifyCommandResult>
    {
        /// <summary>
        /// Builds a new <see cref="ChangeAppointmentDateCommand"/> instance.
        /// </summary>
        /// <param name="data"></param>
        public ChangeAppointmentDateCommand((AppointmentId appointmentId, ZonedDateTime start, ZonedDateTime end) data) : base(Guid.NewGuid(), data)
        {
        }
    }
}
