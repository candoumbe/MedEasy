using MedEasy.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedEasy.DTO
{
    /// <summary>
    /// Request for searching appointment resources.
    /// </summary>
    /// <remarks>
    public class SearchAppointmentInfo : AbstractSearchInfo<AppointmentInfo>
    {
        /// <summary>
        /// Criteria for the <see cref="AppointmentInfo.StartDate"/>.
        /// </summary>
        public DateTimeOffset? From { get; set; }

        /// <summary>
        /// Criteria end date
        /// </summary>
        public DateTimeOffset? To { get; set; }

        /// <summary>
        /// Id of the doctor that is part of the <see cref="AppointmentInfo"/>
        /// </summary>
        public int? DoctorId { get; set; }

        /// <summary>
        /// Id of the patient that is part of the appointment
        /// </summary>
        public int? PatientId { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            IList<ValidationResult> validationsResults = new List<ValidationResult>(base.Validate(validationContext));

            if (From.HasValue && To.HasValue && To.Value.CompareTo(From.Value) < 1)
            {
                validationsResults.Add(new ValidationResult($"Invalid interval of date.", new[] { nameof(From), nameof(To) }));
            }

            return validationsResults;
        }
    }
}
