using Newtonsoft.Json;

namespace MedEasy.DTO
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
