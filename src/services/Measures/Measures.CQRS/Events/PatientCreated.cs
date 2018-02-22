using MedEasy.CQRS.Core.Events;
using System;

namespace Measures.CQRS.Events
{

    /// <summary>
    /// Event that notifies the creation of a new <see cref="PatientInfo"/> resource alongside with a new measure.
    /// </summary>
    public class PatientCreated : NotificationBase<Guid>
    {
        /// <summary>
        /// Created patient's unique identifier
        /// </summary>
        public Guid PatientId { get; }
        /// <summary>
        /// Firstname of the patient
        /// </summary>
        public string Firstname { get; }

        /// <summary>
        /// Patient lastname
        /// </summary>
        public string Lastname { get; }

        /// <summary>
        /// Patient's date of birth
        /// </summary>
        public DateTime? DateOfBirth { get; }

        /// <summary>
        /// Builds a new measure
        /// </summary>
        /// <param name="patientId">Unique identifier of the new patient resource</param>
        /// <param name="firstname">Patient's firstname</param>
        /// <param name="lastname">Patient's lastname</param>
        /// <param name="dateOfBirth">Patient's date of birth</param>
        public PatientCreated(Guid patientId, string firstname, string lastname, DateTime? dateOfBirth) : base(Guid.NewGuid())
        {
            PatientId = patientId;
            Firstname = firstname;
            Lastname = lastname;
            DateOfBirth = dateOfBirth;
        }

    }
}
