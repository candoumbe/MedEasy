using Documents.DTO.v1;
using Documents.Ids;

using MedEasy.CQRS.Core.Queries;

using Optional;

using System;

namespace Documents.CQRS.Queries
{
    public class GetOneDocumentInfoByIdQuery : QueryBase<Guid, DocumentId, Option<DocumentInfo>>
    {
        public GetOneDocumentInfoByIdQuery(DocumentId data) : base(Guid.NewGuid(), data)
        {
        }
    }
}
