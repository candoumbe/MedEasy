namespace Measures.CQRS.Events
{
    using Measures.Ids;

    using MedEasy.CQRS.Core.Events;

    using System;

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
