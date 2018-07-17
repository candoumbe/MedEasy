using MedEasy.CQRS.Core.Events;
using System;

namespace Measures.CQRS.Events
{

    /// <summary>
    /// Event that notifies a <see cref="PatientInfo"/> resource was updated.
    /// </summary>
    public class PatientUpdated : NotificationBase<Guid, Guid>
    {
        /// <summary>
        /// Created patient's unique identifier
        /// </summary>
        public Guid PatientId { get; }

        /// <summary>
        /// Builds a new <see cref="PatientUpdated"/> instance.
        /// </summary>
        /// <param name="patientId"></param>
        public PatientUpdated(Guid patientId) : base(Guid.NewGuid(), patientId)
        { }

    }
}
