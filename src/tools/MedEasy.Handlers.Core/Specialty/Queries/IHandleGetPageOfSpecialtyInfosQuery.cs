using MedEasy.DTO;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries;
using MedEasy.RestObjects;
using System;

namespace MedEasy.Handlers.Core.Specialty.Queries
{
    public interface IHandleGetPageOfSpecialtyInfosQuery: IHandleQueryPageAsync<Guid, SpecialtyInfo, IWantPageOfResources<Guid, SpecialtyInfo>>
    {
    }
}
