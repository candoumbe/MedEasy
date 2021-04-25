using Agenda.DTO;

using MedEasy.CQRS.Core.Commands;

using System;

namespace Agenda.CQRS.Features.Appointments.Commands
{
    /// <summary>
    /// Command to create a new <see cref="AppointmentInfo"/> resource
    /// </summary>
    public class CreateAppointmentInfoCommand : CommandBase<Guid, NewAppointmentInfo, AppointmentInfo>, IEquatable<CreateAppointmentInfoCommand>
    {
        /// <summary>
        /// Builds a new <see cref="CreateAppointmentInfoCommand"/> instance
        /// </summary>
        /// <param name="data">data used to create the <see cref="Appointment"/>resource</param>
        /// <exception cref="ArgumentException"> <paramref name="data"/> is <c>null</c>.</exception>
        public CreateAppointmentInfoCommand(NewAppointmentInfo data) : base(Guid.NewGuid(), data)
        {
        }

        public bool Equals(CreateAppointmentInfoCommand other) =>
            other != null && (ReferenceEquals(this, other) || Equals(Data, other.Data));
    }
}
