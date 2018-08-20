using MedEasy.RestObjects;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace Patients.DTO
{
    /// <summary>
    /// Informations on a patient
    /// </summary>
    [JsonObject]
    public class PatientInfo : Resource<Guid>
    {
        /// <summary>
        /// Patient's firstname
        /// </summary>
        [JsonProperty(PropertyName = nameof(Firstname))]
        [Required]
        [StringLength(255)]
        public string Firstname { get; set; }

        /// <summary>
        /// Patient's lastname
        /// </summary>
        [JsonProperty(PropertyName = nameof(Lastname))]
        [Required]
        [StringLength(255)]
        public string Lastname { get; set; }

        [JsonProperty(PropertyName = nameof(Fullname))]
        public string Fullname => $"{Firstname?.Trim()}{(string.IsNullOrWhiteSpace(Firstname?.Trim()) ? string.Empty : " ")}{Lastname}";

        /// <summary>
        /// Patient's birth date
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Id of the main doctor
        /// </summary>
        public Guid? MainDoctorId { get; set; }

    }
}
