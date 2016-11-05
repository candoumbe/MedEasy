using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MedEasy.DTO
{
    /// <summary>
    /// Informations on a patient
    /// </summary>
    [JsonObject]
    public class PatientInfo : IResource<int>
    {
        /// <summary>
        /// Id of the patient
        /// </summary>
        [JsonProperty]
        public int Id { get; set; }
        /// <summary>
        /// Patient's firstname
        /// </summary>
        [JsonProperty]
        [Required]
        public string Firstname { get; set; }

        /// <summary>
        /// Patient's lastname
        /// </summary>
        [JsonProperty]
        [Required]
        public string Lastname { get; set; }

        /// <summary>
        /// Patient's birthdate
        /// </summary>
        [JsonProperty]
        public DateTimeOffset? BirthDate { get; set; }

        /// <summary>
        /// Patient's birthplace
        /// </summary>
        [JsonProperty]
        [Required(AllowEmptyStrings = true)]
        public string BirthPlace { get; set; }

        /// <summary>
        /// Id of the patient's main doctor
        /// </summary>
        [JsonProperty]
        public int? MainDoctorId { get; set; }


        [JsonProperty]
        public string Fullname { get; set; }

        [JsonProperty]
        public DateTimeOffset? UpdatedDate { get; set; }
        
    }
}
