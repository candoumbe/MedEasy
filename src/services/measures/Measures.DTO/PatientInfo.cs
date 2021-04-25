using Measures.Ids;

using MedEasy.RestObjects;

using NodaTime;

namespace Measures.DTO
{
    /// <summary>
    /// Informations on a patient
    /// </summary>
    public class PatientInfo : Resource<PatientId>
    {
        public string Name { get; set; }

        /// <summary>
        /// Patient's birth date
        /// </summary>
        public LocalDate? BirthDate { get; set; }
    }
}
