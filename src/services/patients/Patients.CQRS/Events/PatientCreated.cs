using System;

using MedEasy.CQRS.Core.Events;

using Patients.DTO;

namespace Patients.CQRS.Events
{
    public class PatientCreated : NotificationBase<Guid, PatientInfo>
    {
        public PatientCreated(PatientInfo data) : base(Guid.NewGuid(), data) { }
    }
}
