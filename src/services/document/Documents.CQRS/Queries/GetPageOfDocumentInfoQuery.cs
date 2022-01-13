namespace Documents.CQRS.Queries
{
    using Documents.DTO.v1;

    using MedEasy.CQRS.Core.Queries;
    using MedEasy.RestObjects;

    using System;

    /// <summary>
    /// Query to retrieve a set of <see cref="DocumentInfo"/>.
    /// </summary>
    public class GetPageOfDocumentInfoQuery : GetPageOfResourcesQuery<Guid, DocumentInfo>
    {
        /// <summary>
        /// Builds a new <see cref="GetPageOfDocumentInfoQuery"/> instance
        /// </summary>
        /// <param name="data">Indicates the set of data to retrieve</param>
        public GetPageOfDocumentInfoQuery(PaginationConfiguration data) : base(Guid.NewGuid(), data.DeepClone())
        {
        }
    }
}
