using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedEasy.ViewModels.Patient
{
    public class PatientEditModel : ModelBase<int>, IValidatableObject
    {
        
        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public DateTimeOffset? BirthDate { get; set; }

        [StringLength(50)]
        public string BirthPlace { get; set; }

        public int? MainDoctorId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (BirthDate.HasValue && BirthDate.Value > DateTimeOffset.UtcNow)
            {
                yield return new ValidationResult($"{BirthDate} not valid", new[] { $"{BirthDate}" });
            }
        }
    }
}
