using Measures.DTO;
using MedEasy.CQRS.Core.Queries;
using Optional;
using System;

namespace Measures.CQRS.Queries.Patients
{
    /// <summary>
    /// Query to read a <see cref="GenericMeasureInfo"/> resource by its <see cref="GenericMeasureInfo.Id"/>.
    /// </summary>
    /// <remarks>
    /// Execution of a <see cref="GetOneMeasureInfoByPatientIdQuery"/> returns an <see cref="Option{GenericMeasureInfo}"/>
    /// </remarks>
    public class GetOneMeasureInfoByPatientIdQuery : GetOneResourceQuery<Guid, (Guid patientId, string name, Guid measureId), Option<GenericMeasureInfo>>
    {
        /// <summary>
        /// Builds a new <see cref="GetPatientInfoByIdQuery"/> instance.
        /// </summary>
        /// <param name="input">id of the resource to read</param>
        public GetOneMeasureInfoByPatientIdQuery((Guid patientId, string name, Guid measureId) input) : base(Guid.NewGuid(), input)
        {
        }
    }
}
