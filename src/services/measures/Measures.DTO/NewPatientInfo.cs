using Newtonsoft.Json;

using NodaTime;

using System;
using System.ComponentModel.DataAnnotations;

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
        public string Name { get; set; }

        /// <summary>
        /// Patient's birth date
        /// </summary>
        [JsonProperty]
        public LocalDate? BirthDate { get; set; }
    }
}
