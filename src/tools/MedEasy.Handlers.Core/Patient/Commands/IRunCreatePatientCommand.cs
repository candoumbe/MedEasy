using MedEasy.Commands.Patient;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Commands;
using System;

namespace MedEasy.Handlers.Core.Patient.Commands
{
    public interface IRunCreatePatientCommand : IRunCommandAsync<Guid, CreatePatientInfo, PatientInfo, ICreatePatientCommand>
    {

    }

}
