using MedEasy.CQRS.Core.Commands;
using Patients.DTO;
using System;

namespace Patients.CQRS.Patient.Commands
{
    /// <summary>
    /// Basic shape of a command to create a new <see cref="PatientInfo"/> resource.
    /// </summary>
    public interface ICreatePatientCommand : ICommand<Guid, CreatePatientInfo, PatientInfo>
    {
        
    }
}
