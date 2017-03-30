using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries.Specialty;
using System;

namespace MedEasy.Handlers.Core.Specialty.Queries
{
    public interface IHandleFindDoctorsBySpecialtyIdQuery : IHandleQueryAsync<Guid, FindDoctorsBySpecialtyIdQueryArgs, IPagedResult<DoctorInfo>, IFindDoctorsBySpecialtyIdQuery>
    {
    }
}
