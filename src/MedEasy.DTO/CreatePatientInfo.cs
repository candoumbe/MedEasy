using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using static System.ComponentModel.DataAnnotations.DataType;

namespace MedEasy.DTO
{
    /// <summary>
    /// Informations to give to create a new <see cref="PatientInfo"/> resource
    /// </summary>
    [JsonObject]
    public class CreatePatientInfo
    {
        
        [JsonProperty]
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
        /// When the patient was born
        /// </summary>
        [JsonProperty]
        [DataType(Date)]
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Where the patient was born ?
        /// </summary>
        [StringLength(255)]
        [JsonProperty]
        public string BirthPlace { get; set; }

        /// <summary>
        /// Id of the patient's main doctor
        /// </summary>
        [JsonProperty]
        [Range(0, int.MaxValue)]
        public Guid? MainDoctorId { get; set; }
        
        
    }
}
