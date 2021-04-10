using Documents.DTO.v1;

using MedEasy.CQRS.Core.Queries;

using System;
using System.Collections.Generic;

namespace Documents.CQRS.Queries
{
    public class GetOneDocumentFileInfoByIdQuery : QueryBase<Guid, Guid, IAsyncEnumerable<DocumentPartInfo>>
    {
        public GetOneDocumentFileInfoByIdQuery(Guid data) : base(Guid.NewGuid(), data)
        {
        }
    }
}
