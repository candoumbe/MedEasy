using MedEasy.RestObjects;
using System;
using System.ComponentModel.DataAnnotations;

namespace Patients.DTO
{
    /// <summary>
    /// Data to create a new <see cref="DoctorInfo"/> resource.
    /// </summary>
    public class CreateDoctorInfo : Resource<Guid>
    {
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
    }
}
