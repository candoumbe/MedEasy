namespace Measures.DTO
{
    using Measures.Ids;

    using System.ComponentModel.DataAnnotations;

    public sealed class BodyWeightInfo : PhysiologicalMeasurementInfo<BodyWeightId>
    {
        /// <summary>
        /// Value of the measure
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal Value { get; set; }
    }
}
