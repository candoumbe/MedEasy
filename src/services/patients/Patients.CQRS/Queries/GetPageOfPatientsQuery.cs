using MedEasy.CQRS.Core.Queries;
using MedEasy.RestObjects;

using Patients.DTO;

using System;

namespace Patients.CQRS.Queries
{
    /// <summary>
    /// Query to retrieve a page of <see cref="PatientInfo"/>.
    /// </summary>
    /// <see cref="GetPageOfResourcesQuery{TQueryId, TResult}"/>
    public class GetPageOfPatientsQuery : GetPageOfResourcesQuery<Guid, PatientInfo>
    {
        /// <summary>
        /// Builds a new <see cref="GetPageOfPatientsQuery"/> instance
        /// </summary>
        /// <param name="pagination">Configuration of pagination</param>
        public GetPageOfPatientsQuery(PaginationConfiguration pagination) : base(Guid.NewGuid(), pagination)
        {
        }
    }
}
