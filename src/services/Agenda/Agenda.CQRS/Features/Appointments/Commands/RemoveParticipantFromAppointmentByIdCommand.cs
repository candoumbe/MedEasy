using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;
using System;

namespace Agenda.CQRS.Features.Appointments.Commands
{
    /// <summary>
    /// Command to add an existing <see cref="DTO.ParticipantInfo"/> to an existing <see cref="DTO.AppointmentInfo"/>
    /// </summary>
    public class RemoveParticipantFromAppointmentByIdCommand : CommandBase<Guid, (Guid appointmentId, Guid participantId), DeleteCommandResult>, IEquatable<RemoveParticipantFromAppointmentByIdCommand>
    {
        /// <summary>
        /// Builds a new <see cref="RemoveParticipantFromAppointmentByIdCommand"/> instance
        /// </summary>
        /// <param name="data">holds the participant id and the appointment id</param>
        public RemoveParticipantFromAppointmentByIdCommand((Guid appointmentId, Guid participantId) data): base(Guid.NewGuid(), data)
        {
            if (data.appointmentId == default)
            {
                throw new ArgumentException($"{nameof(data.appointmentId)} cannot be set to its default value", nameof(data.appointmentId));
            }

            if (data.participantId == default)
            {
                throw new ArgumentException($"{nameof(data.participantId)} cannot be set to its default value", nameof(data.participantId));
            }
        }

        public bool Equals(RemoveParticipantFromAppointmentByIdCommand other) => other != null
                && (ReferenceEquals(this, other) || Equals(Data, other.Data));
    }
}
