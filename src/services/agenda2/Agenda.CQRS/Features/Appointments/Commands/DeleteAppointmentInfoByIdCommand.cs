using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;
using System;

namespace Agenda.CQRS.Features.Appointments.Commands
{
    /// <summary>
    /// Command to delete an <see cref="DTO.AppointmentInfo"/> by its <see cref="MedEasy.RestObjects.Resource{T}.Id"/>
    /// </summary>
    public class DeleteAppointmentInfoByIdCommand : CommandBase<Guid, Guid, DeleteCommandResult>
    {
        /// <summary>
        /// Builds a new <see cref="DeleteAppointmentInfoByIdCommand"/> instance
        /// </summary>
        /// <param name="appointmentId"></param>
        public DeleteAppointmentInfoByIdCommand(Guid appointmentId) : base(Guid.NewGuid(), appointmentId)
        {
        }
    }
}
