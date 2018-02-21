using Measures.DTO;
using MedEasy.CQRS.Core.Queries;
using MedEasy.RestObjects;
using System;

namespace Measures.CQRS.Commands
{
    /// <summary>
    /// Query to get a <see cref="MedEasy.DAL.Repositories.Page{T}"/> of <see cref="BloodPressureInfo"/>s.
    /// </summary>
    public class PageOfBloodPressureInfoQuery : GetPageOfResourcesQuery<Guid, BloodPressureInfo>
    {
        /// <summary>
        /// Builds a new <see cref="PageOfBloodPressureInfoQuery"/> instance.
        /// </summary>
        /// <param name="pagination">The paging configuration</param>
        public PageOfBloodPressureInfoQuery(PaginationConfiguration pagination) : base(Guid.NewGuid(), pagination)
        {
        }
    }
}