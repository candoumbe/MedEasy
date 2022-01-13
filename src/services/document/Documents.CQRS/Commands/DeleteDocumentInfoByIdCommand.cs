namespace Documents.CQRS.Commands
{
    using Documents.Ids;

    using MedEasy.CQRS.Core.Commands;
    using MedEasy.CQRS.Core.Commands.Results;

    using System;

    /// <summary>
    ///  A command to delete a <see cref="Objects.Document"/>
    /// </summary>
    public class DeleteDocumentInfoByIdCommand : CommandBase<Guid, DocumentId, DeleteCommandResult>
    {
        /// <summary>
        /// Builds a new <see cref="DeleteDocumentInfoByIdCommand"/> instance.
        /// </summary>
        /// <param name="id">Identities the document to delete</param>
        /// <exception cref="ArgumentException">if <paramref name="id"/> is <see cref="Empty"/>.</exception>
        public DeleteDocumentInfoByIdCommand(DocumentId id) : base(Guid.NewGuid(), id)
        {
            if (id == DocumentId.Empty)
            {
                throw new ArgumentException(null, nameof(id));
            }
        }
    }
}
