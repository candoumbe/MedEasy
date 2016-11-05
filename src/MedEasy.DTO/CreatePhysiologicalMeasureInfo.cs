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
    public abstract class CreatePhysiologicalMeasureInfo
    {
        
        /// <summary>
        /// When the measure was taken
        /// </summary>
        public DateTimeOffset DateOfMeasure { get; set; }

    }
}
