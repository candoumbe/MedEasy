namespace Patients.CQRS.Commands
{
    using MedEasy.CQRS.Core.Commands;
    using MedEasy.CQRS.Core.Commands.Results;

    using Patients.DTO;
    using Patients.Ids;

    using System;

    /// <summary>
    /// Command to create a new <see cref="PatientInfo"/>
    /// </summary>
    public class DeletePatientInfoByIdCommand : CommandBase<Guid, PatientId, DeleteCommandResult>
    {
        /// <summary>
        /// Creates a new <see cref="CreatePatientInfoCommand"/> instance
        /// </summary>
        /// <param name="id">Id of the resource to delete</param>
        /// <exception cref="ArgumentNullException">if <paramref name="id"/> is <c>null</c></exception>
        public DeletePatientInfoByIdCommand(PatientId id) : base(Guid.NewGuid(), id)
        {
        }
    }
}
