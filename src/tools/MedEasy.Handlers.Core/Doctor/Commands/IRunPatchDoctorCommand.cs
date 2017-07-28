using MedEasy.Commands;
using MedEasy.CQRS.Core;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Commands;
using MedEasy.Handlers.Core.Exceptions;
using Optional;
using System;

namespace MedEasy.Handlers.Core.Doctor.Commands
{
    public interface IRunPatchDoctorCommand : IRunCommandAsync<Guid, IPatchInfo<Guid, Objects.Doctor>, Nothing, IPatchCommand<Guid, Objects.Doctor>>
    {

    }

}
