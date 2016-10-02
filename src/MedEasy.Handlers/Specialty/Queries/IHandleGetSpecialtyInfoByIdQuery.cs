using MedEasy.DTO;
using MedEasy.Handlers.Queries;
using MedEasy.Queries;
using System;

namespace MedEasy.Handlers.Specialty.Queries
{
    public interface IHandleGetSpecialtyInfoByIdQuery : IHandleQueryAsync<Guid, int, SpecialtyInfo, IWantOneResource<Guid, int, SpecialtyInfo>>
    {
    }
}
