namespace Measures.CQRS.Events
{
    using Measures.DTO;

    using MedEasy.CQRS.Core.Events;

    using System;

    /// <summary>
    /// Event that notifies the creation of a new <see cref="PatientInfo"/> resource alongside with a new measure.
    /// </summary>
    public class PatientCreated : NotificationBase<Guid, PatientInfo>
    {
        public PatientCreated(PatientInfo patient) : base(Guid.NewGuid(), patient)
        {
        }
    }
}
