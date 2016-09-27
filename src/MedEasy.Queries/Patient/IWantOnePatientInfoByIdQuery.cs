using MedEasy.DTO;
using System;

namespace MedEasy.Queries.Patient
{
    /// <summary>
    /// Gets a <see cref="PatientInfo"/> by its <see cref="PatientInfo.Id"/>
    /// </summary>
    public interface IWantOnePatientInfoByIdQuery : IWantOneResource<Guid, int, PatientInfo>
    { 
    }


    
}
