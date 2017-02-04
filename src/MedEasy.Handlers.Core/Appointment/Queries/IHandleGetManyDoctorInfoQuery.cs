using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries;
using MedEasy.RestObjects;
using System;

namespace MedEasy.Handlers.Core.Appointment.Queries
{
    public interface IHandleGetManyAppointmentInfosQuery: IHandleQueryAsync<Guid, PaginationConfiguration, IPagedResult<AppointmentInfo>, IWantManyResources<Guid, AppointmentInfo>>
    {
    }
}
