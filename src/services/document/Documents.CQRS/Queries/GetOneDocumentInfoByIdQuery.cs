namespace Documents.CQRS.Queries
{
    using Documents.DTO.v1;
    using Documents.Ids;

    using MedEasy.CQRS.Core.Queries;

    using Optional;

    using System;

    /// <summary>
    /// Query to retrieve a <see cref="DocumentInfo"/> by its id.
    /// </summary>
    public class GetOneDocumentInfoByIdQuery : QueryBase<Guid, DocumentId, Option<DocumentInfo>>
    {
        /// <summary>
        /// Builds a new <see cref="GetOneDocumentFileInfoByIdQuery"/> instance.
        /// </summary>
        /// <param name="data">Identifier of the <see cref="DocumentInfo"/> to retrieve./param>
        public GetOneDocumentInfoByIdQuery(DocumentId data) : base(Guid.NewGuid(), data)
        {
        }
    }
}
