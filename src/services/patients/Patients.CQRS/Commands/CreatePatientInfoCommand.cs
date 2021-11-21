namespace Patients.CQRS.Commands
{
    using MedEasy.CQRS.Core.Commands;
    using MedEasy.CQRS.Core.Commands.Results;

    using Optional;

    using Patients.DTO;

    using System;

    /// <summary>
    /// Command to create a new <see cref="PatientInfo"/>
    /// </summary>
    public class CreatePatientInfoCommand : CommandBase<Guid, CreatePatientInfo, Option<PatientInfo, CreateCommandFailure>>
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
