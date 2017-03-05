using MedEasy.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedEasy.DTO.Search
{
    /// <summary>
    /// Request for searching appointment resources.
    /// </summary>
    /// <remarks>
    public class SearchAppointmentInfo : AbstractSearchInfo<AppointmentInfo>
    {
        /// <summary>
        /// Criterion for the <see cref="AppointmentInfo.StartDate"/>.
        /// </summary>
        public DateTimeOffset? From { get; set; }

        /// <summary>
        /// Criterion end date
        /// </summary>
        public DateTimeOffset? To { get; set; }

        /// <summary>
        /// Id of the doctor that is part of the <see cref="AppointmentInfo"/>
        /// </summary>
        public Guid? DoctorId { get; set; }

        /// <summary>
        /// Id of the patient that is part of the appointment
        /// </summary>
        public Guid? PatientId { get; set; }

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
