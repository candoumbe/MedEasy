using MedEasy.Commands;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Commands;
using System;

namespace MedEasy.Handlers.Core.Doctor.Commands
{
    public interface IRunPatchDoctorCommand : IRunCommandAsync<Guid, IPatchInfo<int, Objects.Doctor>, IPatchCommand<int, Objects.Doctor>>
    {

    }

}
