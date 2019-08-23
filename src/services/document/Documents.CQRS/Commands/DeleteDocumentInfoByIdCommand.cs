using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace Documents.CQRS.Commands
{
    /// <summary>
    ///  A <see cref="CommandBase{Guid, NewDocumentInfo, Option}"/> implementation to delete a new document
    /// </summary>
    public class DeleteDocumentInfoByIdCommand : CommandBase<Guid, Guid, DeleteCommandResult>
    {
        /// <summary>
        /// Builds a new <see cref="DeleteDocumentInfoByIdCommand"/> instance
        /// </summary>
        /// <param name="id">data to create a new <see cref="DocumentInfo"/> resource</param>
        /// <exception cref="ArgumentException">if <paramref name="data"/> is <see cref="Guid.Empty"/>.</exception>
        public DeleteDocumentInfoByIdCommand(Guid id) : base(Guid.NewGuid(), id)
        {
        }
    }
}
