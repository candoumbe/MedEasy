using Newtonsoft.Json;

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
        public int PatientId { get; set; }

        /// <summary>
        /// Id of the measure to get
        /// </summary>
        public int MeasureId { get; set; }
    }
}
