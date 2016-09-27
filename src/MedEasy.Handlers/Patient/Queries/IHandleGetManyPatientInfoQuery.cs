using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.Handlers.Queries;
using MedEasy.Queries;
using MedEasy.Queries.Patient;
using MedEasy.RestObjects;
using System;

namespace MedEasy.Handlers.Patient.Queries
{
    public interface IHandleGetManyPatientInfosQuery: IHandleQueryAsync<Guid, GenericGetQuery, IPagedResult<PatientInfo>, IWantManyResources<Guid, PatientInfo>>
    {
    }
}
