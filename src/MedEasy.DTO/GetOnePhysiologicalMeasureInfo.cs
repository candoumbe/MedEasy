using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

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
        [Range(1, int.MaxValue)]
        public int PatientId { get; set; }

        /// <summary>
        /// Id of the measure to get
        /// </summary>
        [Range(1, int.MaxValue)]
        public int MeasureId { get; set; }
    }
}
