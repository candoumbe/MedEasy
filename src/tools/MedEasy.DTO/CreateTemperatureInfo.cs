using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace MedEasy.DTO
{
    /// <summary>
    /// data to provide when creating a new temperature info
    /// </summary>
    [JsonObject]
    public class CreateTemperatureInfo 
    {
        [DataType(DataType.DateTime)]
        public DateTimeOffset DateOfMeasure { get; set; }

        /// <summary>
        /// The new temperature value
        /// </summary>
        public float Value { get; set; }

        

    }
}
