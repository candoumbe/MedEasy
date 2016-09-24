using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.Handlers.Queries;
using MedEasy.Queries;
using MedEasy.Queries.Doctor;
using MedEasy.RestObjects;
using System;

namespace MedEasy.Handlers.Doctor.Queries
{
    public interface IHandleGetManyDoctorInfosQuery: IHandleQueryAsync<Guid, GenericGetQuery, IPagedResult<DoctorInfo>, IWantManyResources<Guid, DoctorInfo>>
    {
    }
}
