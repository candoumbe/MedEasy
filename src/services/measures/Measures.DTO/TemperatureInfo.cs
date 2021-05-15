namespace Measures.DTO
{
    using Measures.Ids;

    public class TemperatureInfo : PhysiologicalMeasurementInfo<TemperatureId>
    {
        /// <summary>
        /// Value of the measure
        /// </summary>
        public float Value { get; set; }
    }
}
