using MedEasy.RestObjects;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Measures.DTO
{
    /// <summary>
    /// data to provide when creating a new patient resource
    /// </summary>
    [JsonObject]
    public class NewPatientInfo
    {
        /// <summary>
        /// Id of the resource to create
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// Patient's firstname
        /// </summary>
        [JsonProperty]
        [Required]
        [StringLength(255)]
        public string Firstname { get; set; }

        /// <summary>
        /// Patient's lastname
        /// </summary>
        [JsonProperty]
        [Required]
        [StringLength(255)]
        public string Lastname { get; set; }

        /// <summary>
        /// Patient's birth date
        /// </summary>
        [JsonProperty]
        public DateTime? BirthDate { get; set; }
    }
}
