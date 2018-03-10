using MedEasy.RestObjects;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Measures.DTO
{
    /// <summary>
    /// data to provide when creating a new patient resource
    /// </summary>
    [DataContract]
    public class CreatePatientInfo
    {
        /// <summary>
        /// Id of the resource to create
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// Patient's firstname
        /// </summary>
        [DataMember]
        [Required]
        [StringLength(255)]
        public string Firstname { get; set; }

        /// <summary>
        /// Patient's lastname
        /// </summary>
        [DataMember]
        [Required]
        [StringLength(255)]
        public string Lastname { get; set; }

        /// <summary>
        /// Patient's birth date
        /// </summary>
        [DataMember]
        public DateTime? BirthDate { get; set; }

    }
}
