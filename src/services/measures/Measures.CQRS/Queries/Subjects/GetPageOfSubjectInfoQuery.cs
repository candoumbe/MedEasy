namespace Measures.CQRS.Queries.Subjects
{
    using Measures.DTO;

    using MedEasy.CQRS.Core.Queries;
    using MedEasy.RestObjects;

    using System;

    /// <summary>
    /// Query to get a <see cref="MedEasy.DAL.Repositories.Page{T}"/> of <see cref="SubjectInfo"/>s.
    /// </summary>
    public class GetPageOfSubjectInfoQuery : GetPageOfResourcesQuery<Guid, SubjectInfo>
    {
        /// <summary>
        /// Builds a new <see cref="GetPageOfSubjectInfoQuery"/> instance.
        /// </summary>
        /// <param name="pagination">The paging configuration</param>
        public GetPageOfSubjectInfoQuery(PaginationConfiguration pagination) : base(Guid.NewGuid(), pagination)
        {
        }
    }
}