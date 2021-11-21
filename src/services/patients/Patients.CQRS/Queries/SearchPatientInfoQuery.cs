namespace Patients.CQRS.Queries
{
    using MedEasy.CQRS.Core.Queries;
    using MedEasy.DAL.Repositories;

    using Patients.DTO;

    using System;


    /// <summary>
    /// Query to search for <see cref="PatientInfo"/>s.
    /// </summary>
    public class SearchPatientInfoQuery : QueryBase<Guid, SearchPatientInfo, Page<PatientInfo>>
    {
        public SearchPatientInfoQuery(SearchPatientInfo data) : base(Guid.NewGuid(), data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }
        }
    }
}
