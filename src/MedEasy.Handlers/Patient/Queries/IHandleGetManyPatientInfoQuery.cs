using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.Handlers.Queries;
using MedEasy.Queries;
using MedEasy.RestObjects;
using System;

namespace MedEasy.Handlers.Patient.Queries
{
    public interface IHandleGetManyPatientInfosQuery: IHandleQueryAsync<Guid, PaginationConfiguration, IPagedResult<PatientInfo>, IWantManyResources<Guid, PatientInfo>>
    {
    }
}
