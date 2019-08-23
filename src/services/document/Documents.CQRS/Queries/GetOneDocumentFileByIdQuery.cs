using Documents.DTO.v1;
using MedEasy.CQRS.Core.Queries;
using Optional;
using System;

namespace Documents.CQRS.Queries
{
    public class GetOneDocumentFileInfoByIdQuery : QueryBase<Guid, Guid, Option<DocumentFileInfo>>
    {
        public GetOneDocumentFileInfoByIdQuery(Guid data) : base(Guid.NewGuid(), data)
        {
        }
    }
}
