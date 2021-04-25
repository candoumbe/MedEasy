using Measures.DTO;
using Measures.Ids;

using MedEasy.CQRS.Core.Queries;

using Optional;

using System;

namespace Measures.CQRS.Queries.BloodPressures
{
    /// <summary>
    /// Query to read a <see cref="BloodPressureInfo"/> resource by its id.
    /// </summary>
    /// <remarks>
    /// Execution of a <see cref="GetBloodPressureInfoByIdQuery"/> returns an <see cref="Option{BloodPressureInfo}"/>
    /// </remarks>
    public class GetBloodPressureInfoByIdQuery : GetOneResourceQuery<Guid, BloodPressureId, Option<BloodPressureInfo>>
    {
        /// <summary>
        /// Builds a new <see cref="GetBloodPressureInfoByIdQuery"/> instance.
        /// </summary>
        /// <param name="id">id of the resource to read</param>
        public GetBloodPressureInfoByIdQuery(BloodPressureId id) : base(Guid.NewGuid(), id)
        {
        }
    }
}
