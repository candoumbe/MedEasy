using Documents.DTO.v1;

using MedEasy.CQRS.Core.Queries;
using MedEasy.RestObjects;

using System;

namespace Documents.CQRS.Queries
{
    public class GetPageOfDocumentInfoQuery : GetPageOfResourcesQuery<Guid, DocumentInfo>
    {
        public GetPageOfDocumentInfoQuery(PaginationConfiguration data) : base(Guid.NewGuid(), data.DeepClone())
        {
        }
    }
}
