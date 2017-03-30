using MedEasy.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.DTO
{

    /// <summary>
    /// Base class for data to provide when creating any physiological measure informations
    /// </summary>
    [JsonObject]
    public class CreatePhysiologicalMeasureInfo<TPhysiologicalMeasure> where TPhysiologicalMeasure : PhysiologicalMeasurement 
    {
        /// <summary>
        /// Id of the patient for which the measure is created
        /// </summary>
        public Guid PatientId { get; set; }
        

        public TPhysiologicalMeasure Measure { get; set; }

    }
}
