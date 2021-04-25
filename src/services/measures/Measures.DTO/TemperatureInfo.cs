using Measures.Ids;

namespace Measures.DTO
{
    public class TemperatureInfo : PhysiologicalMeasurementInfo<TemperatureId>
    {
        /// <summary>
        /// Value of the measure
        /// </summary>
        public float Value { get; set; }
    }
}
