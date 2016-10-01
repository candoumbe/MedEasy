using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.DTO
{
    /// <summary>
    /// Encapsulates data for when creating a new temperature info
    /// </summary>
    [JsonObject]
    public class CreateTemperatureInfo
    {
        /// <summary>
        /// Id of the patient the temperature is created for
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// The new value
        /// </summary>
        public float Value { get; set; }

        public DateTime Timestamp { get; set; }

    }
}
