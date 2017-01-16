using MedEasy.Commands;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Commands;
using System;

namespace MedEasy.Handlers.Core.Patient.Commands
{
    public interface IRunPatchPatientCommand : IRunCommandAsync<Guid, IPatchInfo<int, Objects.Patient>, IPatchCommand<int, Objects.Patient>>
    {

    }

}
