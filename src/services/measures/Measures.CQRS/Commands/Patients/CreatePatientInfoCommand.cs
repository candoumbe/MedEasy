using Measures.DTO;

using MedEasy.CQRS.Core.Commands;

using System;

namespace Measures.CQRS.Commands.Patients
{
    /// <summary>
    /// Command to create <see cref="PatientInfo"/> resources.
    /// </summary>
    public class CreatePatientInfoCommand : CommandBase<Guid, NewPatientInfo, PatientInfo>
    {
        /// <summary>
        /// Creates a new <see cref="CreatePatientInfoCommand"/> resource 
        /// </summary>
        /// <param name="data">informations on the resource to create</param>
        public CreatePatientInfoCommand(NewPatientInfo data) : base(Guid.NewGuid(), data)
        {
        }
    }
}
