namespace Documents.CQRS.Commands
{
    using Documents.Ids;

    using MedEasy.CQRS.Core.Commands;
    using MedEasy.CQRS.Core.Commands.Results;

    using System;

    /// <summary>
    ///  A <see cref="CommandBase{Guid, NewDocumentInfo, Option}"/> implementation to delete a new document
    /// </summary>
    public class DeleteDocumentInfoByIdCommand : CommandBase<Guid, DocumentId, DeleteCommandResult>
    {
        /// <summary>
        /// Builds a new <see cref="DeleteDocumentInfoByIdCommand"/> instance
        /// </summary>
        /// <param name="id">data to create a new <see cref="DocumentInfo"/> resource</param>
        /// <exception cref="ArgumentException">if <paramref name="data"/> is <see cref="Empty"/>.</exception>
        public DeleteDocumentInfoByIdCommand(DocumentId id) : base(Guid.NewGuid(), id)
        {
            if (id == DocumentId.Empty)
            {
                throw new ArgumentException(null, nameof(id));
            }
        }
    }
}
