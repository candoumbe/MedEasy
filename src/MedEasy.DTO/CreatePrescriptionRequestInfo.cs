using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MedEasy.DTO
{
    /// <summary>
    /// Wraps data to create a new <see cref="PrescriptionHeaderInfo"/>
    /// </summary>
    public class CreatePrescriptionInfo : IValidatableObject
    {
        /// <summary>
        /// When the prescription was delivered
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        public DateTimeOffset DeliveryDate { get; set; }
        /// <summary>
        /// Number of days the <see cref="PrescriptionInfo"/> will be valid (starting from the <see cref="DeliveryDate"/>).
        /// </summary>
        /// <remarks>
        /// This defines how long from <see cref="DeliveryDate"/> the current prescription will be valid.
        /// </remarks>
        [Range(0, double.MaxValue)]
        public double Duration { get; set; }

        /// <summary>
        /// Content of the prescription
        /// </summary>
        public IEnumerable<PrescriptionItemInfo> Items { get; set; }

        /// <summary>
        /// Id of the doctor who created the prescription
        /// </summary>
        public Guid PrescriptorId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!Items?.Any() ?? true)
            {
                yield return new ValidationResult("No prescription items");
            }
        }
    }
}
