using MedEasy.DTO;
using System;

namespace MedEasy.Queries.Patient
{
    /// <summary>
    /// Immutable class to query <see cref="DTO.PatientInfo"/> by specifying its <see cref="DTO.PatientInfo.Id"/>
    /// </summary>
    public class WantOneTemperatureMeasureQuery : IWantOneTemperatureMesureQuery
    {
        public Guid Id { get; }

        public GetOnePhysiologicalMeasureInfo Data { get; }

        /// <summary>
        /// Builds a new <see cref="WantOneTemperatureMeasureQuery"/> instance
        /// </summary>
        /// <param name="id">unique id of the <see cref="PatientInfo"/> to retrieve temperature measure from</param>
        /// <param name="temperatureId">unique id of the temperature to retrieve</param>
        public WantOneTemperatureMeasureQuery(int id, int temperatureId)
        {
            Id = Guid.NewGuid();
            Data = new GetOnePhysiologicalMeasureInfo { PatientId = id, MeasureId = temperatureId };
        }

        
    }
}