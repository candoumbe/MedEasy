using Measures.DTO;
using MedEasy.CQRS.Core.Queries;
using Optional;
using System;

namespace Measures.CQRS.Queries.Patients
{
    /// <summary>
    /// Query to read a <see cref="GenericMeasureFormInfo"/> resource by its <see cref="GenericMeasureFormInfo.Id"/>.
    /// </summary>
    /// <remarks>
    /// Execution of a <see cref="GetOneMeasureFormByIdQuery"/> returns an <see cref="Option{GenericMeasureFormInfo}"/>
    /// </remarks>
    public class GetOneMeasureFormByIdQuery : GetOneResourceQuery<Guid, Guid, Option<GenericMeasureFormInfo>>
    {
        /// <summary>
        /// Builds a new <see cref="GetPatientInfoByIdQuery"/> instance.
        /// </summary>
        /// <param name="input">id of the resource to read</param>
        public GetOneMeasureFormByIdQuery(Guid id) : base(Guid.NewGuid(), id)
        {
        }
    }
}
