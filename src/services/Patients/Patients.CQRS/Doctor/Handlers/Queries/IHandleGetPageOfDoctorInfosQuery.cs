using MedEasy.CQRS.Core.Handlers.Queries;
using MedEasy.CQRS.Core.Queries;
using MedEasy.RestObjects;
using Patients.DTO;
using System;

namespace Patients.CQRS.Doctor.Handlers.Queries
{
    public interface IHandleGetPageOfDoctorInfosQuery: IHandleQueryPageAsync<Guid, PaginationConfiguration, DoctorInfo, IWantPage<Guid, DoctorInfo>>
    {
    }
}
