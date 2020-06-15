using Measures.DTO;
using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;

using Optional;

using System;

namespace Measures.CQRS.Queries.Patients
{
    /// <summary>
    /// Query to get a page of <see cref="GenericMeasureInfo"/> for a patient.
    /// Only the <see cref="GenericMeasureInfo"/>s with the specified name will be returned.
    /// </summary>
    public class GetPageOfMeasuresInfoByPatientIdQuery : QueryBase<Guid,(Guid patientId, string name, PaginationConfiguration pagination), Option<Page<GenericMeasureInfo>>>
    {
        /// <summary>
        /// Builds a new <see cref="GetPageOfPatientInfoQuery"/> instance.
        /// </summary>
        /// <param name="input"></param>
        public GetPageOfMeasuresInfoByPatientIdQuery((Guid patientId, string name, PaginationConfiguration) input) : base(Guid.NewGuid(), input)
        {
        }

        public override string ToString() => this.Jsonify();
    }
}