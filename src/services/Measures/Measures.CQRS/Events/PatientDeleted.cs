using MedEasy.CQRS.Core.Events;
using System;

namespace Measures.CQRS.Events
{

    /// <summary>
    /// Event that notifies a patient resource was updated.
    /// </summary>
    public class PatientDeleted : NotificationBase<Guid, Guid>
    {
        

        /// <summary>
        /// Builds a new measure
        /// </summary>
        /// <param name="patientId">Unique identifier of the new patient resource</param>
        public PatientDeleted(Guid patientId) : base(Guid.NewGuid(), patientId)
        {
        }

    }
}
