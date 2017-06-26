using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.RestObjects;
using System;

namespace MedEasy.Queries.Doctor
{
    /// <summary>
    /// Request many <see cref="DoctorInfo"/>s
    /// </summary>
    public interface IWantManyDoctorInfoQuery : IWantPageOfResources<Guid, DoctorInfo>
    {
    }
}
