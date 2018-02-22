using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;
using System;

namespace Measures.CQRS.Commands.Patients
{
    /// <summary>
    /// Command to delete a new <see cref="PatientInfo"/>.
    /// </summary>
    public class DeletePatientInfoByIdCommand : CommandBase<Guid, Guid, DeleteCommandResult>
    {
        /// <summary>
        /// Builds a new <see cref="DeletePatientInfoByIdCommand"/> instance
        /// </summary>
        /// <param name="data">id of the <see cref="PatientInfo"/> resource to delete</param>
        public DeletePatientInfoByIdCommand(Guid data) : base(Guid.NewGuid(), data)
        {

        }
    }
}