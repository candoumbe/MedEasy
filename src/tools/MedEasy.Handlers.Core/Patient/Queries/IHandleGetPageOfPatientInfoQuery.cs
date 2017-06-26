using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries;
using MedEasy.RestObjects;
using System;

namespace MedEasy.Handlers.Core.Patient.Queries
{
    public interface IHandleGetPageOfPatientInfosQuery: IHandleQueryPageAsync<Guid, PatientInfo, IWantPageOfResources<Guid, PatientInfo>>
    {
    }
}
