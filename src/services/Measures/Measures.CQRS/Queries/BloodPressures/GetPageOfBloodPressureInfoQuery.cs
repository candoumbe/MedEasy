using Measures.DTO;
using MedEasy.CQRS.Core.Queries;
using MedEasy.RestObjects;
using System;

namespace Measures.CQRS.Queries.BloodPressures
{
    /// <summary>
    /// Query to get a <see cref="MedEasy.DAL.Repositories.Page{T}"/> of <see cref="BloodPressureInfo"/>s.
    /// </summary>
    public class GetPageOfBloodPressureInfoQuery : GetPageOfResourcesQuery<Guid, BloodPressureInfo>
    {
        /// <summary>
        /// Builds a new <see cref="GetPageOfBloodPressureInfoQuery"/> instance.
        /// </summary>
        /// <param name="pagination">The paging configuration</param>
        public GetPageOfBloodPressureInfoQuery(PaginationConfiguration pagination) : base(Guid.NewGuid(), pagination)
        {
        }
    }
}