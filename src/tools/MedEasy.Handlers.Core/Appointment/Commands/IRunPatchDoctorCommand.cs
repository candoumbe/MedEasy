using MedEasy.Commands;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Commands;
using MedEasy.Handlers.Core.Exceptions;
using Optional;
using System;

namespace MedEasy.Handlers.Core.Appointment.Commands
{
    public interface IRunPatchAppointmentCommand : IRunCommandAsync<Guid, IPatchInfo<Guid, Objects.Appointment>, IPatchCommand<Guid, Objects.Appointment>>
    {

    }

}
