using MedEasy.CQRS.Core;
using MedEasy.CQRS.Core.Commands;
using MedEasy.DTO;
using System;

namespace Patients.CQRS.Doctor.Handlers.Commands
{
    public interface IRunPatchDoctorCommand : IRunCommandAsync<Guid, PatchInfo<Guid, Objects.Doctor>, Nothing, IPatchCommand<Guid, Objects.Doctor>>
    {

    }

}
