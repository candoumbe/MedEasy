using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MedEasy.CQRS.Core.Events;
using MediatR;
using Patients.DTO;

namespace Patients.CQRS.Events
{
    public class PatientCreated : NotificationBase<Guid, PatientInfo>
    {
        public PatientCreated(PatientInfo data) : base(Guid.NewGuid(), data) { }
    }
}
