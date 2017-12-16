using MedEasy.CQRS.Core.Commands;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Commands;
using System;

namespace Patients.CQRS.Commands
{
    public interface IRunPatchPatientCommand : IRunCommandAsync<Guid, PatchInfo<Guid, Objects.Patient>, IPatchCommand<Guid, Objects.Patient>>
    {

    }

}
