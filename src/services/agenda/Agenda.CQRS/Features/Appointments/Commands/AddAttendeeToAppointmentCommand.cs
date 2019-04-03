using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;
using System;

namespace Agenda.CQRS.Features.Appointments.Commands
{
    /// <summary>
    /// Command to add an existing <see cref="DTO.AttendeeInfo"/> to an existing <see cref="DTO.AppointmentInfo"/>
    /// </summary>
    public class AddAttendeeToAppointmentCommand : CommandBase<Guid, (Guid appointmentId, Guid attendeeId), ModifyCommandResult>, IEquatable<AddAttendeeToAppointmentCommand>
    {
        /// <summary>
        /// Builds a new <see cref="AddAttendeeToAppointmentCommand"/> instance
        /// </summary>
        /// <param name="data">holds the participant id and the appointment id</param>
        public AddAttendeeToAppointmentCommand((Guid appointmentId, Guid attendeeId) data): base(Guid.NewGuid(), data)
        {

        }

        public override bool Equals(object obj) => Equals(obj as AddAttendeeToAppointmentCommand);

        public bool Equals(AddAttendeeToAppointmentCommand other) => other != null
                && Data.Equals(other.Data);

        public override int GetHashCode() => Data.GetHashCode();
    }
}
