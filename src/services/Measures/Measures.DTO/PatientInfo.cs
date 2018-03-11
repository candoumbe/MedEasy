using MedEasy.RestObjects;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Measures.DTO
{
    /// <summary>
    /// Informations on a patient
    /// </summary>
    [DataContract]
    public class PatientInfo : Resource<Guid>
    {
        /// <summary>
        /// Patient's firstname
        /// </summary>
        [DataMember(Name = nameof(Firstname))]
        [Required]
        [StringLength(100)]
        public string Firstname { get; set; }

        /// <summary>
        /// Patient's lastname
        /// </summary>
        [DataMember(Name = nameof(Lastname))]
        [Required]
        [StringLength(100)]
        public string Lastname { get; set; }

        [DataMember(Name = nameof(Fullname))]
        public string Fullname { get; set; }

        /// <summary>
        /// Patient's birth date
        /// </summary>
        public DateTime? BirthDate { get; set; }

    }
}
