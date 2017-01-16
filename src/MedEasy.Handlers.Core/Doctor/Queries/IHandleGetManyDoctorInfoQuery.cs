using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries;
using MedEasy.RestObjects;
using System;

namespace MedEasy.Handlers.Core.Doctor.Queries
{
    public interface IHandleGetManyDoctorInfosQuery: IHandleQueryAsync<Guid, PaginationConfiguration, IPagedResult<DoctorInfo>, IWantManyResources<Guid, DoctorInfo>>
    {
    }
}
