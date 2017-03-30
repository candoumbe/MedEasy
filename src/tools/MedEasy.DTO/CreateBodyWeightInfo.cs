using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace MedEasy.DTO
{
    /// <summary>
    /// data to provide when creating a new <see cref="BodyWeightInfo"/>.
    /// </summary>
    [JsonObject]
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
