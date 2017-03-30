using MedEasy.DTO;
using System;
using System.Collections.Generic;

namespace MedEasy.Queries.Patient
{
    /// <summary>
    /// Gets most recents <see cref="TOutput"/>s by its <see cref="PatientInfo.Id"/>
    /// </summary>
    /// <typeparam name="TOutput">Type of the instance to return when handling the query</typeparam>
    public interface IWantMostRecentPhysiologicalMeasuresQuery<TOutput> : IWantOneResource<Guid, GetMostRecentPhysiologicalMeasuresInfo, IEnumerable<TOutput>>
        where TOutput : PhysiologicalMeasurementInfo
    { 
    }


    
}
