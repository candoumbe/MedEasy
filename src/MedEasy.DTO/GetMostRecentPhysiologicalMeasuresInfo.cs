using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.DTO
{
    /// <summary>
    /// Data to provide when querying many physiological measure information
    /// </summary>
    [JsonObject]
    public class GetMostRecentPhysiologicalMeasuresInfo
    {
        /// <summary>
        /// Id of the patient to get one measure from
        /// </summary>
        [Range(1, int.MaxValue)]
        public int PatientId { get; set; }

        /// <summary>
        /// Number of measures to return
        /// </summary>
        [Range(0, int.MaxValue)]
        public int? Count { get; set; }
    }
}
