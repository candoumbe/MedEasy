using MedEasy.DTO.Search;
using MedEasy.RestObjects;

using NodaTime;

namespace Measures.DTO
{
    /// <summary>
    /// Wraps seearch criteria for <see cref="PatientInfo"/> resources.
    /// </summary>
    public class SearchPatientInfo : AbstractSearchInfo<PatientInfo>
    {
        /// <summary>
        /// Searched <see cref="Name"/>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Searched <see cref="BirthDate"/>
        /// </summary>
        [FormField(Type = FormFieldType.Date)]
        public LocalDate? BirthDate { get; set; }
    }
}
