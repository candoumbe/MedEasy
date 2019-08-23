using Documents.DTO.v1;
using MedEasy.CQRS.Core.Queries;
using Optional;
using System;

namespace Documents.CQRS.Queries
{
    public class GetOneDocumentInfoByIdQuery : QueryBase<Guid, Guid, Option<DocumentInfo>>
    {
        public GetOneDocumentInfoByIdQuery(Guid data) : base(Guid.NewGuid(), data)
        {
        }
    }
}
