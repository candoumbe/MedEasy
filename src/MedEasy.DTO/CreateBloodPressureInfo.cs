using Newtonsoft.Json;

namespace MedEasy.DTO
{
    /// <summary>
    /// data to provide when creating a new blood pressure info
    /// </summary>
    [JsonObject]
    public class CreateBloodPressureInfo : CreatePhysiologicalMeasureInfo
    {
        /// <summary>
        /// The new systolic blod pressure value
        /// </summary>
        public float SystolicPressure { get; set; }


        /// <summary>
        /// The new diastolic blod pressure value
        /// </summary>
        public float DiastolicPressure { get; set; }



    }
}
