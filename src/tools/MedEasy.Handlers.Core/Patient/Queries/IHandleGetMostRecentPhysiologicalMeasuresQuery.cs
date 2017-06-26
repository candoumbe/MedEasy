using MedEasy.DTO;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries;
using MedEasy.Queries.Patient;
using System;
using System.Collections.Generic;

namespace MedEasy.Handlers.Core.Patient.Queries
{
    /// <summary>
    /// Base interface for handlers of most recent physiological measurement
    /// </summary>
    /// <typeparam name="TPhysiologicalMeasurement">Type of the physiological measurement that will be handled</typeparam>
    public interface IHandleGetMostRecentPhysiologicalMeasuresQuery<TPhysiologicalMeasurement> : IHandleQueryOneAsync<Guid, GetMostRecentPhysiologicalMeasuresInfo, IEnumerable<TPhysiologicalMeasurement>, IWantMostRecentPhysiologicalMeasuresQuery<TPhysiologicalMeasurement>>
        where TPhysiologicalMeasurement : PhysiologicalMeasurementInfo
    {
    }
}
