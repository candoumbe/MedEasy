using Measures.Ids;

using MedEasy.CQRS.Core.Events;

using System;

namespace Measures.CQRS.Events
{
    /// <summary>
    /// Event that notifies a <see cref="PatientInfo"/> resource was updated.
    /// </summary>
    public class PatientUpdated : NotificationBase<Guid, PatientId>
    {
        /// <summary>
        /// Created patient's unique identifier
        /// </summary>
        public PatientId PatientId { get; }

        /// <summary>
        /// Builds a new <see cref="PatientUpdated"/> instance.
        /// </summary>
        /// <param name="patientId"></param>
        public PatientUpdated(PatientId patientId) : base(Guid.NewGuid(), patientId)
        { }
    }
}
