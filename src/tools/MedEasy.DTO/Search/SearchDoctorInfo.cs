using MedEasy.DTO;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedEasy.DTO
{
    /// <summary>
    /// Request for searching doctor resources.
    /// </summary>
    /// <remarks>
    public class SearchDoctorInfo : AbstractSearchInfo<DoctorInfo>
    {
        /// <summary>
        /// Criteria for the <see cref="Firstname"/>.
        /// </summary>
        /// <remarks>
        /// Can be :
        ///  
        ///     "Bruce" to match all Doctor where the firstname is exactly "Bruce"
        ///    
        ///     "B*e" to match all resources
        /// </remarks>
        public string Firstname { get; set; }

        /// <summary>
        /// Criteria for the lastname
        /// </summary>
        public string Lastname { get; set; }


        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            IList<ValidationResult> validationsResults = new List<ValidationResult>(base.Validate(validationContext));

            if (string.IsNullOrWhiteSpace(Firstname) && string.IsNullOrWhiteSpace(Lastname))
            {
                validationsResults.Add(new ValidationResult("One of the search criteria must be set.", new[] { nameof(Firstname), nameof(Lastname) }));
            }

            return validationsResults;
        }


    }
}
