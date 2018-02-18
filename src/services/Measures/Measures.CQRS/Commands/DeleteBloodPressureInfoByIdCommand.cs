using Measures.DTO;
using MedEasy.CQRS.Core.Commands;
using MediatR;
using System;

namespace Measures.CQRS.Commands
{
    /// <summary>
    /// Command to delete a new <see cref="BloodPressureInfo"/>.
    /// </summary>
    public class DeleteBloodPressureInfoByIdCommand : CommandBase<Guid, Guid, DeleteCommandResult>
    {
        /// <summary>
        /// Builds a new <see cref="DeleteBloodPressureInfoByIdCommand"/> instance
        /// </summary>
        /// <param name="data">id of the <see cref="BloodPressureInfo"/> resource to delete</param>
        public DeleteBloodPressureInfoByIdCommand(Guid data) : base(Guid.NewGuid(), data)
        {

        }
    }
}