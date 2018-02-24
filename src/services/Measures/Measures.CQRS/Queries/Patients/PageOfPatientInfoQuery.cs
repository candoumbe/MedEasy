using Measures.DTO;
using MedEasy.CQRS.Core.Queries;
using MedEasy.RestObjects;
using System;

namespace Measures.CQRS.Queries.Patients
{
    /// <summary>
    /// Query to get a <see cref="MedEasy.DAL.Repositories.Page{T}"/> of <see cref="PatientInfo"/>s.
    /// </summary>
    public class PageOfPatientInfoQuery : GetPageOfResourcesQuery<Guid, PatientInfo>
    {
        /// <summary>
        /// Builds a new <see cref="PageOfPatientInfoQuery"/> instance.
        /// </summary>
        /// <param name="pagination">The paging configuration</param>
        public PageOfPatientInfoQuery(PaginationConfiguration pagination) : base(Guid.NewGuid(), pagination)
        {
        }
    }
}