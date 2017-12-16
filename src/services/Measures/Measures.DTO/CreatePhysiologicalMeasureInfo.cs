using System;
using Measures.Objects;
namespace Measures.DTO
{

    /// <summary>
    /// Base class for data to provide when creating any physiological measure informations
    /// </summary>
    public class CreatePhysiologicalMeasureInfo<TPhysiologicalMeasure> where TPhysiologicalMeasure : PhysiologicalMeasurement 
    {
        /// <summary>
        /// Id of the patient for which the measure is created
        /// </summary>
        public Guid PatientId { get; set; }
        

        public TPhysiologicalMeasure Measure { get; set; }

    }
}
