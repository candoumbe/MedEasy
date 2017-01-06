using MedEasy.Commands;
using MedEasy.Commands.Patient;
using MedEasy.DTO;
using MedEasy.Handlers.Commands;
using System;
using System.Collections.Generic;

namespace MedEasy.Handlers.Doctor.Commands
{
    public interface IRunPatchDoctorCommand : IRunCommandAsync<Guid, IPatchInfo<int, Objects.Doctor>, IPatchCommand<int, Objects.Doctor>>
    {

    }

}
