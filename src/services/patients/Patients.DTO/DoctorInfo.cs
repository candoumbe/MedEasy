using MedEasy.RestObjects;

using Newtonsoft.Json;

using Patients.Ids;

using System.ComponentModel.DataAnnotations;

namespace Patients.DTO
{
    /// <summary>
    /// Informations on a patient
    /// </summary>
    public class DoctorInfo : Resource<DoctorId>
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

        public string Fullname { get; set; }
    }
}
