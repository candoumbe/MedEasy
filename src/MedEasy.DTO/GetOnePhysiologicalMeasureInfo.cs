using Newtonsoft.Json;
using System;

namespace MedEasy.DTO
{
    /// <summary>
    /// Data to provide when querying any physiological measure information
    /// </summary>
    [JsonObject]
    public class GetOnePhysiologicalMeasureInfo
    {
        /// <summary>
        /// Id of the patient to get one measure from
        /// </summary>
        public Guid PatientId { get; set; }

        /// <summary>
        /// Id of the measure to get
        /// </summary>
        public Guid MeasureId { get; set; }
    }
}
