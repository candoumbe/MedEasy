using MedEasy.Objects;
using System;

namespace Measures.Objects
{
    /// <summary>
    /// Patient that owns measures data
    /// </summary>
    public class Patient : AuditableEntity<int, Patient>
    {
        /// <summary>
        /// Patient's firstname
        /// </summary>
        public string Firstname { get; set; }

        /// <summary>
        /// Patient"s lastname
        /// </summary>
        public string Lastname { get; set; }

        /// <summary>
        /// Patient's date of birth
        /// </summary>
        public DateTime? BirthDate { get; set; }

    }
}