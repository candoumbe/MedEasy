using MedEasy.CQRS.Core.Queries;
using MedEasy.Handlers.Core.Queries;
using Patients.DTO;
using System;

namespace Patients.CQRS.Doctor.Handlers.Queries
{

    public interface IHandleGetDoctorInfoByIdQuery : IHandleQueryOneAsync<Guid, Guid, DoctorInfo, IWantOne<Guid, Guid, DoctorInfo>>
    {
    }
}