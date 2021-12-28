namespace Measures.CQRS.Events
{
    using Measures.DTO;

    using MedEasy.CQRS.Core.Events;

    using System;

    /// <summary>
    /// Event that notifies the creation of a new <see cref="SubjectInfo"/> resource alongside with a new measure.
    /// </summary>
    public class PatientCreated : NotificationBase<Guid, SubjectInfo>
    {
        public PatientCreated(SubjectInfo patient) : base(Guid.NewGuid(), patient)
        {
        }
    }
}
