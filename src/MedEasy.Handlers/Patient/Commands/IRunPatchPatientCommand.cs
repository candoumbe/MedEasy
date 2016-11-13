using MedEasy.Commands;
using MedEasy.Commands.Patient;
using MedEasy.DTO;
using MedEasy.Handlers.Commands;
using System;
using System.Collections.Generic;

namespace MedEasy.Handlers.Patient.Commands
{
    public interface IRunPatchPatientCommand : IRunCommandAsync<Guid, IPatchInfo<int>, PatientInfo, IPatchCommand<int>>

    {

    }

}
