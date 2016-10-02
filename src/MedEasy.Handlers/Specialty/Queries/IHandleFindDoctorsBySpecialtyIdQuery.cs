using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.Handlers.Queries;
using MedEasy.Queries.Specialty;
using System;

namespace MedEasy.Handlers.Specialty.Queries
{
    public interface IHandleFindDoctorsBySpecialtyIdQuery : IHandleQueryAsync<Guid, FindDoctorsBySpecialtyIdQueryArgs, IPagedResult<DoctorInfo>, IFindDoctorsBySpecialtyIdQuery>
    {
    }
}
