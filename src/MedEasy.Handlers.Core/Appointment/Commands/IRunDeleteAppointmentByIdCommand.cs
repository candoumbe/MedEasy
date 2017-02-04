using MedEasy.Commands.Appointment;
using MedEasy.Handlers.Core.Commands;
using System;

namespace MedEasy.Handlers.Core.Appointment.Commands
{
    public interface IRunDeleteAppointmentInfoByIdCommand : IRunCommandAsync<Guid, int, IDeleteAppointmentByIdCommand>
    {

    }

}
