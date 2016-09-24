using MedEasy.DTO;
using System;

namespace MedEasy.Queries.Doctor
{
    /// <summary>
    /// Gets a <see cref="DoctorInfo"/> by its <see cref="DoctorInfo.Id"/>
    /// </summary>
    public interface IWantOneDoctorInfoByIdQuery : IWantOneResource<Guid, int, DoctorInfo>
    { 
    }


    
}
