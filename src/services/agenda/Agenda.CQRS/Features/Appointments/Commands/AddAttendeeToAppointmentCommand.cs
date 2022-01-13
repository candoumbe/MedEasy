namespace Agenda.CQRS.Features.Appointments.Commands
{
    using Agenda.Ids;

    using MedEasy.CQRS.Core.Commands;
    using MedEasy.CQRS.Core.Commands.Results;

    using System;

    /// <summary>
    /// Command to add an existing <see cref="DTO.AttendeeInfo"/> to an existing <see cref="DTO.AppointmentInfo"/>
    /// </summary>
    public class AddAttendeeToAppointmentCommand : CommandBase<Guid, (AppointmentId appointmentId, AttendeeId attendeeId), ModifyCommandResult>, IEquatable<AddAttendeeToAppointmentCommand>
    {
        /// <summary>
        /// Builds a new <see cref="AddAttendeeToAppointmentCommand"/> instance
        /// </summary>
        /// <param name="data">holds the participant id and the appointment id</param>
        public AddAttendeeToAppointmentCommand((AppointmentId appointmentId, AttendeeId attendeeId) data) : base(Guid.NewGuid(), data)
        {
        }

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as AddAttendeeToAppointmentCommand);

        ///<inheritdoc/>
        public bool Equals(AddAttendeeToAppointmentCommand other) => other != null
                && Data.Equals(other.Data);

        ///<inheritdoc/>
        public override int GetHashCode() => Data.GetHashCode();
    }
}
