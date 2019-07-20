using System;
using System.ComponentModel.DataAnnotations;

namespace Measures.DTO
{
    /// <summary>
    /// data to provide when creating a new temperature info
    /// </summary>
    public class CreateTemperatureInfo : CreatePhysiologicalMeasureInfo
    {
        [DataType(DataType.DateTime)]
        public DateTimeOffset DateOfMeasure { get; set; }

        /// <summary>
        /// The new temperature value
        /// </summary>
        public float Value { get; set; }

    }
}
