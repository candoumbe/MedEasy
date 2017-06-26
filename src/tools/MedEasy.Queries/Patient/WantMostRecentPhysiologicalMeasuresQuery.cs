using MedEasy.DTO;
using System;

namespace MedEasy.Queries.Patient
{
    /// <summary>
    /// Immutable class to query <see cref="DTO.PatientInfo"/> by specifying its <see cref="DTO.PatientInfo.Id"/>
    /// </summary>
    /// <typeparam name="TOutput">Type of the instance to return when handling the query</typeparam>
    public class WantMostRecentPhysiologicalMeasuresQuery<TOutput> : IWantMostRecentPhysiologicalMeasuresQuery<TOutput>
         where TOutput : PhysiologicalMeasurementInfo
    {
        public Guid Id { get; }

        public GetMostRecentPhysiologicalMeasuresInfo Data { get; }

        /// <summary>
        /// Builds a new <see cref="WantOnePhysiologicalMeasureQuery{TOutput}"/> instance
        /// </summary>
        /// <param name="id">unique id of the <see cref="PatientInfo"/> to retrieve temperature measure from</param>
        /// <param name="measureId">unique id of the measure to retrieve</param>
        public WantMostRecentPhysiologicalMeasuresQuery(GetMostRecentPhysiologicalMeasuresInfo input)
        {
            Data = input ?? throw new ArgumentNullException(nameof(input));
            Id = Guid.NewGuid();
        }

        
    }
}