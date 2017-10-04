using Newtonsoft.Json;
using System;
using static Newtonsoft.Json.JsonConvert;

namespace MedEasy.DTO
{
    /// <summary>
    /// Resource that holds appointment informations.
    /// </summary>
    [JsonObject]
    public class CreateAppointmentInfo
    {
        /// <summary>
        /// Date of the beginning of the appointment
        /// </summary>
        [JsonProperty(PropertyName = nameof(StartDate))]
        public DateTimeOffset StartDate { get; set; }

        /// <summary>
        /// Appointment Duration in minutes.
        /// </summary>
        [JsonProperty(PropertyName = nameof(Duration))]
        public double Duration { get; set; }
        /// <summary>
        /// The patient of the appointment
        /// </summary>
        [JsonProperty(PropertyName = nameof(PatientId))]
        public Guid PatientId { get; set; }

        /// <summary>
        /// Doctor of the appointment
        /// </summary>
        [JsonProperty(PropertyName = nameof(DoctorId))]
        public Guid DoctorId { get; set; }


        public override string ToString() => SerializeObject(this);
    }
}
