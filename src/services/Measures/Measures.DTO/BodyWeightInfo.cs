using System.ComponentModel.DataAnnotations;

namespace Measures.DTO
{
    public class BodyWeightInfo : PhysiologicalMeasurementInfo
    {
        
        /// <summary>
        /// Value of the measure
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal Value { get; set; }

    }
}
