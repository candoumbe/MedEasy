using MedEasy.CQRS.Core.Commands;
using Patients.CQRS.Patient.Commands;
using Patients.DTO;
using System;

namespace Patients.CQRS.Patient.Handlers.Commands
{
    public interface IRunCreatePatientCommand : IRunCommandAsync<Guid, CreatePatientInfo, PatientInfo, ICreatePatientCommand>
    {

    }

}
