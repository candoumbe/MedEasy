using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;
using System;

namespace Agenda.CQRS.Features.Appointments.Commands
{
    /// <summary>
    /// Command to add an existing <see cref="DTO.AttendeeInfo"/> to an existing <see cref="DTO.AppointmentInfo"/>
    /// </summary>
    public class RemoveAttendeeFromAppointmentByIdCommand : CommandBase<Guid, (Guid appointmentId, Guid attendeeId), DeleteCommandResult>, IEquatable<RemoveAttendeeFromAppointmentByIdCommand>
    {
        /// <summary>
        /// Builds a new <see cref="RemoveAttendeeFromAppointmentByIdCommand"/> instance
        /// </summary>
        /// <param name="data">holds the participant id and the appointment id</param>
        public RemoveAttendeeFromAppointmentByIdCommand((Guid appointmentId, Guid attendeeId) data): base(Guid.NewGuid(), data)
        {
            if (data.appointmentId == default)
            {
                throw new ArgumentException($"{nameof(data.appointmentId)} cannot be set to its default value", nameof(data.appointmentId));
            }

            if (data.attendeeId == default)
            {
                throw new ArgumentException($"{nameof(data.attendeeId)} cannot be set to its default value", nameof(data.attendeeId));
            }
        }

        public bool Equals(RemoveAttendeeFromAppointmentByIdCommand other) => other != null
                && (ReferenceEquals(this, other) || Equals(Data, other.Data));
    }
}
