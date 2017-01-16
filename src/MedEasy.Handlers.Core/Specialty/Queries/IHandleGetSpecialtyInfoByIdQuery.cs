using MedEasy.DTO;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries;
using System;

namespace MedEasy.Handlers.Core.Specialty.Queries
{
    public interface IHandleGetSpecialtyInfoByIdQuery : IHandleQueryAsync<Guid, int, SpecialtyInfo, IWantOneResource<Guid, int, SpecialtyInfo>>
    {
    }
}
