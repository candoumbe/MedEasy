namespace Documents.CQRS.Queries
{
    using Documents.DTO.v1;
    using Documents.Ids;

    using MedEasy.CQRS.Core.Queries;

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Query to get a <see cref="DocumentPartInfo"/> by its <see cref="DocumentId"/>
    /// </summary>
    public class GetOneDocumentFileInfoByIdQuery : QueryBase<Guid, DocumentId, IAsyncEnumerable<DocumentPartInfo>>
    {
        /// <summary>
        /// Builds a new <see cref="GetOneDocumentFileInfoByIdQuery"/> instance
        /// </summary>
        /// <param name="data">Identifier of the document to lookup for</param>
        public GetOneDocumentFileInfoByIdQuery(DocumentId data) : base(Guid.NewGuid(), data)
        {
        }
    }
}
