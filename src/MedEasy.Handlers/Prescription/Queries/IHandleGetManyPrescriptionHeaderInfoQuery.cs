using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.Handlers.Queries;
using MedEasy.Queries;
using MedEasy.RestObjects;
using System;

namespace MedEasy.Handlers.Prescription.Queries
{
    public interface IHandleGetManyPrescriptionHeaderInfosQuery : IHandleQueryAsync<Guid, GenericGetQuery, IPagedResult<PrescriptionHeaderInfo>, IWantManyResources<Guid, PrescriptionHeaderInfo>>
    {
    }
}
