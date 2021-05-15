namespace Measures.DTO
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// data to provide when creating a new <see cref="BodyWeightInfo"/>.
    /// </summary>
    public class CreateBodyWeightInfo
    {
        /// <summary>
        /// Weight value
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal Value { get; set; }

        [DataType(DataType.DateTime)]
        public DateTimeOffset DateOfMeasure { get; set; }
    }
}
