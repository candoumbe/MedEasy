using MedEasy.Objects;
using System;

namespace Patients.Objects
{
    /// <summary>
    /// Patient entity
    /// </summary>
    public class Patient : AuditableEntity<int, Patient>
    {
        /// <summary>
        /// Firstname
        /// </summary>
        public string Firstname { get; set; }
        
        /// <summary>
        /// Lastname
        /// </summary>
        public string Lastname { get; set; }

        /// <summary>
        /// When the patient was born
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Where the patient was born
        /// </summary>
        public string BirthPlace { get; set; }
    }
}
