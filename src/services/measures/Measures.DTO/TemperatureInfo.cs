using Newtonsoft.Json;

namespace Measures.DTO
{
    [JsonObject]
    public class TemperatureInfo : PhysiologicalMeasurementInfo
    {
        /// <summary>
        /// Value of the measure
        /// </summary>
        public float Value { get; set; }
    }
}
