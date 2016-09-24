using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedEasy.ViewModels.Patient
{
    public class PatientCreateModel : ModelBase<int>, IValidatableObject
    {
        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public DateTime? BirthDate { get; set; }

        [StringLength(50)]
        public string BirthPlace { get; set; }

        [StringLength(255)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        public int? MainDoctorId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (BirthDate.HasValue && BirthDate.Value > DateTime.Now)
            {
                yield return new ValidationResult($"{BirthDate} not valid", new[] { $"{BirthDate}" });
            }
        }
    }
}
