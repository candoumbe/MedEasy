namespace Patients.CQRS.Events
{
    using System;

    using MedEasy.CQRS.Core.Events;

    using Patients.DTO;

    public class PatientCreated : NotificationBase<Guid, PatientInfo>
    {
        public PatientCreated(PatientInfo data) : base(Guid.NewGuid(), data) { }
    }
}
