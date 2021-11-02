namespace Patients.DTO
{

    using NodaTime;

    using Patients.Ids;

    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Data to create a new <see cref="PatientInfo"/> resource.
    /// </summary>
    public class CreatePatientInfo
    {
        /// <summary>
        /// Patient's identifier
        /// </summary>
        public PatientId Id { get; set; }

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
        public LocalDate? BirthDate { get; set; }

        /// <summary>
        /// Patient's birthplace
        /// </summary>
        public string BirthPlace { get; set; }

        /// <summary>
        /// Id of the <see cref="Doctor"/> the patient consults.
        /// </summary>
        public DoctorId MainDoctorId { get; set; }
    }
}
