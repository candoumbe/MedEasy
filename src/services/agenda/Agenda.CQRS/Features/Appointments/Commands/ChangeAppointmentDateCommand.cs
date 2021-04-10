using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;

using NodaTime;

using System;

namespace Agenda.CQRS.Features.Appointments.Commands
{
    /// <summary>
    /// Command to change an <see cref="Objects.Appointment"/>'s <see cref="Objects.Appointment.StartDate"/>/<see cref="Objects.Appointment.EndDate"/>.
    /// </summary>
    public class ChangeAppointmentDateCommand : CommandBase<Guid, (Guid appointmentId, ZonedDateTime start, ZonedDateTime end), ModifyCommandResult>
    {
        public ChangeAppointmentDateCommand((Guid appointmentId, ZonedDateTime start, ZonedDateTime end) data) : base (Guid.NewGuid(), data)
        {
        }
    }
}
