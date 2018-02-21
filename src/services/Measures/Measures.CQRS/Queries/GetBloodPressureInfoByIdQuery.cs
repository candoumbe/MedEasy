using Measures.DTO;
using MedEasy.CQRS.Core.Queries;
using MediatR;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Measures.CQRS.Queries
{
    /// <summary>
    /// Query to read a <see cref="BloodPressureInfo"/> resource by its id.
    /// </summary>
    /// <remarks>
    /// Execution of a <see cref="GetBloodPressureInfoByIdQuery"/> returns an <see cref="Option{BloodPressureInfo}"/>
    /// </remarks>
    public class GetBloodPressureInfoByIdQuery : GetOneResourceQuery<Guid, Guid, Option<BloodPressureInfo>>
    {
        /// <summary>
        /// Builds a new <see cref="GetBloodPressureInfoByIdQuery"/> instance.
        /// </summary>
        /// <param name="id">id of the resource to read</param>
        public GetBloodPressureInfoByIdQuery(Guid id) : base(Guid.NewGuid(), id)
        {
        }
    }
}
