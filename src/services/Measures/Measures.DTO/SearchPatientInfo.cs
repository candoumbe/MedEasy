using MedEasy.DTO.Search;
using MedEasy.RestObjects;
using System;

namespace Measures.DTO
{
    /// <summary>
    /// Wraps seearch criteria for <see cref="PatientInfo"/> resources.
    /// </summary>
    public class SearchPatientInfo : AbstractSearchInfo<PatientInfo>
    {
        /// <summary>
        /// Searched <see cref="Firstname"/>
        /// </summary>
        public string Firstname { get; set; }

        /// <summary>
        /// Searched <see cref="Lastname"/>
        /// </summary>
        public string Lastname { get; set; }

        /// <summary>
        /// Searched <see cref="BirthDate"/>
        /// </summary>
        [FormField(Type = FormFieldType.Date)]
        public DateTime? BirthDate { get; set; }
    }
}
