using MedEasy.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.API.Models
{
    /// <summary>
    /// Request for searching patient resources.
    /// </summary>
    /// <remarks>
    public class SearchPatientRequestModel : AbstractSearchRequestModel<PatientInfo>
    {
        /// <summary>
        /// Criteria for the <see cref="Firstname"/>.
        /// </summary>
        /// <remarks>
        /// Can be :
        ///  
        ///     "Bruce" to match all Patient where the firstname is exactly "Bruce"
        ///    
        ///     "B*e" to match all resources
        /// </remarks>
        public string Firstname { get; set; }

        public string Lastname { get; set; }


        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(Firstname))
            {

            }

            return base.Validate(validationContext);
        }


    }
}
