using Agenda.Ids;

using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;

using System;

namespace Agenda.CQRS.Features.Appointments.Commands
{
    /// <summary>
    /// Command to delete an <see cref="DTO.AppointmentInfo"/> by its <see cref="MedEasy.RestObjects.Resource{T}.Id"/>
    /// </summary>
    public class DeleteAppointmentInfoByIdCommand : CommandBase<Guid, AppointmentId, DeleteCommandResult>
    {
        /// <summary>
        /// Builds a new <see cref="DeleteAppointmentInfoByIdCommand"/> instance
        /// </summary>
        /// <param name="appointmentId"></param>
        public DeleteAppointmentInfoByIdCommand(AppointmentId appointmentId) : base(Guid.NewGuid(), appointmentId)
        {
            if (appointmentId is null)
            {
                throw new ArgumentNullException(nameof(appointmentId));
            }

            if (appointmentId == AppointmentId.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(appointmentId), appointmentId, $"{nameof(appointmentId)} cannot be empty");
            }
        }
    }
}
