using MedEasy.CQRS.Core.Commands;
using Patients.DTO;
using System;

namespace Patients.CQRS.Commands
{
    /// <summary>
    /// Command to create a new <see cref="PatientInfo"/>
    /// </summary>
    public class CreatePatientInfoCommand : CommandBase<Guid, CreatePatientInfo, PatientInfo>
    {
        /// <summary>
        /// Creates a new <see cref="CreatePatientInfoCommand"/> instance
        /// </summary>
        /// <param name="data">data associated with the command</param>
        /// <exception cref="ArgumentNullException">if <paramref name="data"/> is <c>null</c></exception>
        public CreatePatientInfoCommand(CreatePatientInfo data) : base(Guid.NewGuid(), data)
        {

        }
    }
}
