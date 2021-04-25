using Measures.DTO;
using Measures.Ids;

using MedEasy.CQRS.Core.Queries;

using Optional;

using System;

namespace Measures.CQRS.Queries.Patients
{
    /// <summary>
    /// Query to read a <see cref="PatientInfo"/> resource by its id.
    /// </summary>
    /// <remarks>
    /// Execution of a <see cref="GetPatientInfoByIdQuery"/> returns an <see cref="Option{PatientInfo}"/>
    /// </remarks>
    public class GetPatientInfoByIdQuery : GetOneResourceQuery<Guid, PatientId, Option<PatientInfo>>
    {
        /// <summary>
        /// Builds a new <see cref="GetPatientInfoByIdQuery"/> instance.
        /// </summary>
        /// <param name="id">id of the resource to read</param>
        public GetPatientInfoByIdQuery(PatientId id) : base(Guid.NewGuid(), id)
        {
        }
    }
}
