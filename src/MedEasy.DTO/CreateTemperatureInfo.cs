using Newtonsoft.Json;

namespace MedEasy.DTO
{
    /// <summary>
    /// data to provide when creating a new temperature info
    /// </summary>
    [JsonObject]
    public class CreateTemperatureInfo : CreatePhysiologicalMeasureInfo
    {
        /// <summary>
        /// The new temperature value
        /// </summary>
        public float Value { get; set; }

        

    }
}
