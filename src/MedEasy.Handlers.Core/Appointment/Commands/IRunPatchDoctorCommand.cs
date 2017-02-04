using MedEasy.Commands;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Commands;
using System;

namespace MedEasy.Handlers.Core.Appointment.Commands
{
    public interface IRunPatchAppointmentCommand : IRunCommandAsync<Guid, IPatchInfo<int, Objects.Appointment>, IPatchCommand<int, Objects.Appointment>>
    {

    }

}
