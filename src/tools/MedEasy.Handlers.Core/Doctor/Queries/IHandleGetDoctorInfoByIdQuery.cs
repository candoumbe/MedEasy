using MedEasy.DTO;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries;
using System;

namespace MedEasy.Handlers.Core.Doctor.Queries
{

    public interface IHandleGetDoctorInfoByIdQuery : IHandleQueryOneAsync<Guid, Guid, DoctorInfo, IWantOneResource<Guid, Guid, DoctorInfo>>
    {
    }
}