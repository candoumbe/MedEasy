using MedEasy.RestObjects;

using NodaTime;

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
        public LocalDate? BirthDate { get; set; }
    }
}
