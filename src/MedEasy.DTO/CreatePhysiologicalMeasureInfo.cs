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
        /// Id of the patient to get one measure from
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// When the measure was taken
        /// </summary>
        public DateTime DateOfMeasure { get; set; }

    }
}
