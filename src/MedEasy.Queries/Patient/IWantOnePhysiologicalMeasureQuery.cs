using MedEasy.DTO;
using System;

namespace MedEasy.Queries.Patient
{
    /// <summary>
    /// Gets a <see cref="TOutput"/> by its <see cref="PatientInfo.Id"/>
    /// </summary>
    /// <typeparam name="TOutput">Type of the instance to return when handling the query</typeparam>
    public interface IWantOnePhysiologicalMeasureQuery<TOutput> : IWantOneResource<Guid, GetOnePhysiologicalMeasureInfo, TOutput>
    { 
    }


    
}
