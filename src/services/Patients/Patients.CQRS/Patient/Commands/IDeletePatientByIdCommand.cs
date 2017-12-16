using MedEasy.CQRS.Core.Commands;
using System;

namespace Patients.CQRS.Patient.Commands
{

    /// <summary>
    /// Shape of a command to delete a <see cref="DTO.PatientInfo"/> resource by its id
    /// </summary>
    public interface IDeletePatientByIdCommand : ICommand<Guid, Guid>
    {
        
    }
}
