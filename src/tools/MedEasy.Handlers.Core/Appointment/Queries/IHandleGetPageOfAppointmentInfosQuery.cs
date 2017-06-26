using MedEasy.DTO;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries;
using MedEasy.Queries.Appointment;
using System;

namespace MedEasy.Handlers.Core.Appointment.Queries
{
    public interface IHandleGetPageOfAppointmentInfosQuery: IHandleQueryPageAsync<Guid, AppointmentInfo, IWantPageOfResources<Guid, AppointmentInfo>>
    {
    }
}
