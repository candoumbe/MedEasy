using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries;
using MedEasy.RestObjects;
using System;

namespace MedEasy.Handlers.Core.Prescription.Queries
{
    public interface IHandleGetManyPrescriptionHeaderInfosQuery : IHandleQueryAsync<Guid, PaginationConfiguration, IPagedResult<PrescriptionHeaderInfo>, IWantManyResources<Guid, PrescriptionHeaderInfo>>
    {
    }
}
