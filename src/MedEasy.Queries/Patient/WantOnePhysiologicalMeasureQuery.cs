using MedEasy.DTO;
using System;

namespace MedEasy.Queries.Patient
{
    /// <summary>
    /// Immutable class to query <see cref="DTO.PatientInfo"/> by specifying its <see cref="DTO.PatientInfo.Id"/>
    /// </summary>
    /// <typeparam name="TOutput">Type of the instance to return when handling the query</typeparam>
    public class WantOnePhysiologicalMeasureQuery<TOutput> : IWantOnePhysiologicalMeasureQuery<TOutput>
    {
        public Guid Id { get; }

        public GetOnePhysiologicalMeasureInfo Data { get; }

        /// <summary>
        /// Builds a new <see cref="WantOnePhysiologicalMeasureQuery{TOutput}"/> instance
        /// </summary>
        /// <param name="id">unique id of the <see cref="PatientInfo"/> to retrieve temperature measure from</param>
        /// <param name="measureId">unique id of the measure to retrieve</param>
        public WantOnePhysiologicalMeasureQuery(int id, int measureId)
        {
            Id = Guid.NewGuid();
            Data = new GetOnePhysiologicalMeasureInfo { PatientId = id, MeasureId = measureId };
        }

        
    }
}