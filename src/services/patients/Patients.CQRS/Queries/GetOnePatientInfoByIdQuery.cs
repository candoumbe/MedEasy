namespace Patients.CQRS.Queries
{
    using MedEasy.CQRS.Core.Queries;

    using Optional;

    using Patients.DTO;
    using Patients.Ids;

    using System;

    /// <summary>
    /// Query to get a <see cref="PatientInfo"/> by its <see cref="PatientInfo.Id"/>
    /// </summary>
    public class GetOnePatientInfoByIdQuery : GetOneResourceQuery<Guid, PatientId, Option<PatientInfo>>
    {
        public GetOnePatientInfoByIdQuery(PatientId id) : base(Guid.NewGuid(), id)
        {
        }
    }
}
