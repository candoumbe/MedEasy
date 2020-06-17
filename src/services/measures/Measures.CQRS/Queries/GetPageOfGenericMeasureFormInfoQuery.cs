using Measures.DTO;
using MedEasy.CQRS.Core.Queries;
using MedEasy.RestObjects;
using System;

namespace Measures.CQRS.Queries.Patients
{
    /// <summary>
    /// Query to get a <see cref="MedEasy.DAL.Repositories.Page{T}"/> of <see cref="GenericMeasureFormInfo"/>s.
    /// </summary>
    public class GetPageOfGenericMeasureFormInfoQuery : GetPageOfResourcesQuery<Guid, GenericMeasureFormInfo>
    {
        /// <summary>
        /// Builds a new <see cref="GetPageOfGenericMeasureFormInfoQuery"/> instance.
        /// </summary>
        /// <param name="pagination">The paging configuration</param>
        public GetPageOfGenericMeasureFormInfoQuery(PaginationConfiguration pagination) : base(Guid.NewGuid(), pagination)
        {
        }
    }
}