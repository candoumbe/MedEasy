using Measures.Ids;

using System.ComponentModel.DataAnnotations;

namespace Measures.DTO
{
    public sealed class BodyWeightInfo : PhysiologicalMeasurementInfo<BodyWeightId>
    {
        /// <summary>
        /// Value of the measure
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal Value { get; set; }
    }
}
