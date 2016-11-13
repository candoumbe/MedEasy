using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.DTO
{
    [JsonObject]
    public class BodyWeightInfo : PhysiologicalMeasurementInfo
    {
        
        /// <summary>
        /// Value of the measure
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal Value { get; set; }

    }
}
