using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.DTO
{
    [JsonObject]
    public class BloodPressureInfo : PhysiologicalMeasurementInfo
    {
        
        /// <summary>
        /// Value of the measure
        /// </summary>
        public float SystolicPressure { get; set; }

        /// <summary>
        /// Bloo
        /// </summary>
        public float DiastolicPressure { get; set; }

    }
}
