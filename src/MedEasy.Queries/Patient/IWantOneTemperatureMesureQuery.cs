using MedEasy.DTO;
using System;

namespace MedEasy.Queries.Patient
{
    /// <summary>
    /// Gets a <see cref="TemperatureInfo"/> by its <see cref="PatientInfo.Id"/>
    /// </summary>
    public interface IWantOneTemperatureMesureQuery : IWantOneResource<Guid, GetOnePhysiologicalMeasureInfo, TemperatureInfo>
    { 
    }


    
}
