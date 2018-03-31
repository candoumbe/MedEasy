using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;
using System;

namespace Agenda.CQRS.Features.Appointments.Commands
{
    /// <summary>
    /// Command to add an existing <see cref="DTO.ParticipantInfo"/> to an existing <see cref="DTO.AppointmentInfo"/>
    /// </summary>
    public class AddParticipantToAppointmentCommand : CommandBase<Guid, (Guid appointmentId, Guid participantId), ModifyCommandResult>, IEquatable<AddParticipantToAppointmentCommand>
    {
        /// <summary>
        /// Builds a new <see cref="AddParticipantToAppointmentCommand"/> instance
        /// </summary>
        /// <param name="data">holds the participant id and the appointment id</param>
        public AddParticipantToAppointmentCommand((Guid appointmentId, Guid participantId) data): base(Guid.NewGuid(), data)
        {

        }

        public bool Equals(AddParticipantToAppointmentCommand other) => other != null
                && (ReferenceEquals(this, other) || Equals(Data, other.Data));
    }
}
