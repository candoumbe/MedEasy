namespace Measures.CQRS.Events
{
    using Measures.Ids;

    using MedEasy.CQRS.Core.Events;

    using System;

    /// <summary>
    /// Event that notifies a patient resource was updated.
    /// </summary>
    public class PatientDeleted : NotificationBase<Guid, SubjectId>
    {
        /// <summary>
        /// Builds a new measure
        /// </summary>
        /// <param name="patientId">Unique identifier of the new patient resource</param>
        public PatientDeleted(SubjectId patientId) : base(Guid.NewGuid(), patientId)
        {
        }
    }
}
