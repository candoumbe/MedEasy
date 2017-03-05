﻿using MedEasy.Commands;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Commands;
using System;

namespace MedEasy.Handlers.Core.Patient.Commands
{
    public interface IRunPatchPatientCommand : IRunCommandAsync<Guid, IPatchInfo<Guid, Objects.Patient>, IPatchCommand<Guid, Objects.Patient>>
    {

    }

}
