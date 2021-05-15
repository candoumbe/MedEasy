namespace Measures.CQRS.Queries.Patients
{
    using Measures.DTO;

    using MedEasy.CQRS.Core.Queries;
    using MedEasy.RestObjects;

    using System;

    /// <summary>
    /// Query to get a <see cref="MedEasy.DAL.Repositories.Page{T}"/> of <see cref="PatientInfo"/>s.
    /// </summary>
    public class GetPageOfPatientInfoQuery : GetPageOfResourcesQuery<Guid, PatientInfo>
    {
        /// <summary>
        /// Builds a new <see cref="GetPageOfPatientInfoQuery"/> instance.
        /// </summary>
        /// <param name="pagination">The paging configuration</param>
        public GetPageOfPatientInfoQuery(PaginationConfiguration pagination) : base(Guid.NewGuid(), pagination)
        {
        }
    }
}