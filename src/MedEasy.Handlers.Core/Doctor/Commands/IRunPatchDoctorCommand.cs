using MedEasy.Commands;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Commands;
using System;

namespace MedEasy.Handlers.Core.Doctor.Commands
{
    public interface IRunPatchDoctorCommand : IRunCommandAsync<Guid, IPatchInfo<Guid, Objects.Doctor>, IPatchCommand<Guid, Objects.Doctor>>
    {

    }

}
