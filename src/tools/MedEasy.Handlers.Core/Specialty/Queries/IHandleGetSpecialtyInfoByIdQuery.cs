using MedEasy.DTO;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries;
using System;

namespace MedEasy.Handlers.Core.Specialty.Queries
{
    public interface IHandleGetSpecialtyInfoByIdQuery : IHandleQueryOneAsync<Guid, Guid, SpecialtyInfo, IWantOneResource<Guid, Guid, SpecialtyInfo>>
    {
    }
}
