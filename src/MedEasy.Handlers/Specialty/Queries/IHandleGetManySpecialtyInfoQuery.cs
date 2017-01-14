using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.Handlers.Queries;
using MedEasy.Queries;
using MedEasy.RestObjects;
using System;

namespace MedEasy.Handlers.Specialty.Queries
{
    public interface IHandleGetManySpecialtyInfosQuery: IHandleQueryAsync<Guid, PaginationConfiguration, IPagedResult<SpecialtyInfo>, IWantManyResources<Guid, SpecialtyInfo>>
    {
    }
}
