using MedEasy.Commands;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Commands;
using MedEasy.Handlers.Core.Exceptions;
using Optional;
using System;

namespace MedEasy.Handlers.Core.Patient.Commands
{
    public interface IRunPatchPatientCommand : IRunCommandAsync<Guid, PatchInfo<Guid, Objects.Patient>, IPatchCommand<Guid, Objects.Patient>>
    {

    }

}
