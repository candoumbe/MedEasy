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
    public class PatientInfo : Resource<Guid>
    {
        /// <summary>
        /// Patient's firstname
        /// </summary>
        [JsonProperty(PropertyName = nameof(Firstname))]
        [Required]
        public string Firstname { get; set; }

        /// <summary>
        /// Patient's lastname
        /// </summary>
        [JsonProperty(PropertyName = nameof(Lastname))]
        [Required]
        public string Lastname { get; set; }

        /// <summary>
        /// Patient's birthdate
        /// </summary>
        [JsonProperty(PropertyName = nameof(BirthDate))]
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Patient's birthplace
        /// </summary>
        [JsonProperty(PropertyName = nameof(BirthPlace))]
        [Required(AllowEmptyStrings = true)]
        public string BirthPlace { get; set; }

        /// <summary>
        /// Id of the patient's main doctor
        /// </summary>
        [JsonProperty(PropertyName = nameof(MainDoctorId))]
        public Guid? MainDoctorId { get; set; }


        [JsonProperty(PropertyName = nameof(Fullname))]
        public string Fullname { get; set; }
        
    }
}
