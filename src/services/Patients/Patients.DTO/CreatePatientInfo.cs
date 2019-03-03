using MedEasy.RestObjects;
using System;
using System.ComponentModel.DataAnnotations;

namespace Patients.DTO
{
    /// <summary>
    /// Data to create a new <see cref="PatientInfo"/> resource.
    /// </summary>
    public class CreatePatientInfo 
    {
        /// <summary>
        /// Patient's identifier
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// Patient's first name
        /// </summary>
        [MaxLength(255)]
        public string Firstname { get; set; }

        /// <summary>
        /// Patient's lastname
        /// </summary>
        [MaxLength(255)]
        public string Lastname { get; set; }

        /// <summary>
        /// Patient's birthdate
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Patient's birthplace
        /// </summary>
        public string BirthPlace { get; set; }

        /// <summary>
        /// Id of the <see cref="Doctor"/> the patient consults.
        /// </summary>
        public Guid? MainDoctorId { get; set; }
    }
}
