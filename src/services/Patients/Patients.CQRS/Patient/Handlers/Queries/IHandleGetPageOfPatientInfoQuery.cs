using MedEasy.CQRS.Core.Queries;
using MedEasy.Handlers.Core.Queries;
using Patients.DTO;
using System;

namespace Patients.CQRS.Patient.Handlers.Queries
{
    public interface IHandleGetPageOfPatientInfosQuery: IHandleQueryPageAsync<Guid, PatientInfo, IWantPage<Guid, PatientInfo>>
    {
    }
}
