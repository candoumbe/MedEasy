﻿namespace Patients.DTO
{
    using MedEasy.Ids;
    using MedEasy.RestObjects;

    using NodaTime;

    using Patients.Ids;

    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Informations on a patient
    /// </summary>
    public class PatientInfo : Resource<PatientId>
    {
        /// <summary>
        /// Patient's firstname
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Firstname { get; set; }

        /// <summary>
        /// Patient's lastname
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Lastname { get; set; }

        /// <summary>
        /// Patient's birth date
        /// </summary>
        public LocalDate? BirthDate { get; set; }

        /// <summary>
        /// Id of the main doctor
        /// </summary>
        public DoctorId MainDoctorId { get; set; }

        /// <summary>
        /// Owner of the current resource
        /// </summary>
        public TenantId TenantId { get; set; }
    }
}
