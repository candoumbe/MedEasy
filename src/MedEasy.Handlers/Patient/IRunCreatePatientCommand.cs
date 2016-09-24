using MedEasy.Commands.Patient;
using MedEasy.DTO;
using MedEasy.Handlers.Commands;
using System;

namespace MedEasy.Handlers.Patient.Commands
{
    public interface IRunCreatePatientCommand : IRunCommandAsync<Guid, CreatePatientInfo, PatientInfo, ICreatePatientCommand>
    {

    }

}
