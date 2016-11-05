using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MedEasy.DTO
{
    /// <summary>
    /// Wraps a request for creating a <see cref="DoctorInfo"/>
    /// </summary>
    [JsonObject]
    public class CreateDoctorInfo
    {
        [JsonProperty]
        public string Firstname { get; set; }

        /// <summary>
        /// Doctor's lastname
        /// </summary>
        [Required]
        [JsonProperty]
        public string Lastname { get; set; }

        /// <summary>
        /// Id of the doctor's main specialty
        /// </summary>
        [JsonProperty]
        [Range(1, int.MaxValue)]
        public int? SpecialtyId { get; set; }
    }

}
