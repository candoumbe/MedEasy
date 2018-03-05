using Measures.DTO;
using MedEasy.CQRS.Core.Commands;
using System;

namespace Measures.CQRS.Commands.Patients
{
    /// <summary>
    /// Command to create <see cref="PatientInfo"/> resources.
    /// </summary>
    public class CreatePatientInfoCommand : CommandBase<Guid, CreatePatientInfo, PatientInfo>
    {
        /// <summary>
        /// Creates a new <see cref="CreatePatientInfoCommand"/> resource 
        /// </summary>
        /// <param name="data">informations on the resource to create</param>
        public CreatePatientInfoCommand(CreatePatientInfo data) : base(Guid.NewGuid(), data)
        {
        }
    }
}
