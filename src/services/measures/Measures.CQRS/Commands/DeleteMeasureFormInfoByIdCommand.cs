using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;
using System;

namespace Measures.CQRS.Commands
{
    /// <summary>
    /// Command to delete a new <see cref="DTO.GenericMeasureFormInfo"/>.
    /// </summary>
    public class DeleteMeasureFormInfoByIdCommand : CommandBase<Guid, Guid, DeleteCommandResult>
    {
        /// <summary>
        /// Builds a new <see cref="DeleteMeasureFormInfoByIdCommand"/> instance
        /// </summary>
        /// <param name="data">id of the <see cref="PatientInfo"/> resource to delete</param>
        public DeleteMeasureFormInfoByIdCommand(Guid data) : base(Guid.NewGuid(), data)
        {

        }
    }
}