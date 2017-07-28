using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using static Newtonsoft.Json.JsonConvert;

namespace MedEasy.DTO
{
    /// <summary>
    /// Wraps a request for creating a <see cref="DoctorInfo"/>
    /// </summary>
    [JsonObject]
    public class CreateDoctorInfo
    {
        [StringLength(255)]
        [JsonProperty]
        public string Firstname { get; set; }

        /// <summary>
        /// Doctor's lastname
        /// </summary>
        [StringLength(255)]
        [Required]
        [JsonProperty]
        public string Lastname { get; set; }

        /// <summary>
        /// Id of the doctor's main specialty
        /// </summary>
        [JsonProperty]
        public Guid? SpecialtyId { get; set; }


        public override string ToString() => SerializeObject(this);
    }

}
