namespace Measures.DTO
{
    using MedEasy.DTO.Search;
    using MedEasy.RestObjects;

    using NodaTime;

    /// <summary>
    /// Wraps seearch criteria for <see cref="SubjectInfo"/> resources.
    /// </summary>
    public class SearchPatientInfo : AbstractSearchInfo<SubjectInfo>
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
