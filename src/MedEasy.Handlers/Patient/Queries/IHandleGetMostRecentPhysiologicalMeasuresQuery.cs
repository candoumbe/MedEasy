using MedEasy.DTO;
using MedEasy.Handlers.Queries;
using MedEasy.Objects;
using MedEasy.Queries;
using System;
using System.Collections.Generic;

namespace MedEasy.Handlers.Patient.Queries
{
    /// <summary>
    /// Base interface for handlers of most recent physiological measurement
    /// </summary>
    /// <typeparam name="TPhysiologicalMeasurement">Type of the physiological measurement that will be handled</typeparam>
    public interface IHandleGetMostRecentPhysiologicalMeasuresQuery<TPhysiologicalMeasurement> : IHandleQueryAsync<Guid, GetMostRecentPhysiologicalMeasuresInfo, IEnumerable<TPhysiologicalMeasurement>, IQuery<Guid, GetMostRecentPhysiologicalMeasuresInfo, IEnumerable<TPhysiologicalMeasurement>>>
        where TPhysiologicalMeasurement : PhysiologicalMeasurementInfo
    {
    }
}
