using MedEasy.Abstractions;
using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;
using System;

namespace Agenda.CQRS.Features.Appointments.Commands
{
    /// <summary>
    /// Command to change an <see cref="Objects.Appointment"/>'s <see cref="Objects.Appointment.StartDate"/>/<see cref="Objects.Appointment.EndDate"/>.
    /// </summary>
    public class RescheduleAppointmentCommand : CommandBase<Guid, (Guid appointmentId, DateTimeOffset start, DateTimeOffset end)>
    {
        public RescheduleAppointmentCommand((Guid appointmentId, DateTimeOffset start, DateTimeOffset end) data) : base (Guid.NewGuid(), data)
        {
        }
    }
}
