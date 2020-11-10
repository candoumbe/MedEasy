using MedEasy.RestObjects;

using System;

namespace Measures.DTO
{
    /// <summary>
    /// Informations on a patient
    /// </summary>
    public class PatientInfo : Resource<Guid>
    {
        public string Name { get; set; }

        /// <summary>
        /// Patient's birth date
        /// </summary>
        public DateTime? BirthDate { get; set; }
    }
}
