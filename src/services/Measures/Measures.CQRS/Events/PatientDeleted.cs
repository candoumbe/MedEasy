using MedEasy.CQRS.Core.Events;
using System;

namespace Measures.CQRS.Events
{

    /// <summary>
    /// Event that notifies a patient resource was updated.
    /// </summary>
    public class PatientDeleted : NotificationBase<Guid>
    {
        /// <summary>
        /// Deleded patient's unique identifier
        /// </summary>
        public Guid PatientId { get; }
        

        /// <summary>
        /// Builds a new measure
        /// </summary>
        /// <param name="patientId">Unique identifier of the new patient resource</param>
        public PatientDeleted(Guid patientId) : base(Guid.NewGuid())
        {
            PatientId = patientId;
        }

    }
}
