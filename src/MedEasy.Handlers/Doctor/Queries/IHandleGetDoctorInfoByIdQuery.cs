using MedEasy.DTO;
using MedEasy.Handlers.Queries;
using MedEasy.Queries;
using System;

namespace MedEasy.Handlers.Doctor.Queries
{

    public interface IHandleGetDoctorInfoByIdQuery : IHandleQueryAsync<Guid, int, DoctorInfo, IWantOneResource<Guid, int, DoctorInfo>>
    {
    }
}