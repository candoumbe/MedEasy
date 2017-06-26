﻿using MedEasy.DTO;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries;
using System;

namespace MedEasy.Handlers.Core.Patient.Queries
{
    /// <summary>
    /// Base interface for handlers of one physiological measurement
    /// </summary>
    /// <typeparam name="TPhysiologicalMeasurement">Type of the physiological measurement that will be handled</typeparam>
    public interface IHandleGetOnePhysiologicalMeasureQuery<TPhysiologicalMeasurement> : IHandleQueryOneAsync<Guid, GetOnePhysiologicalMeasureInfo, TPhysiologicalMeasurement, IWantOneResource<Guid, GetOnePhysiologicalMeasureInfo, TPhysiologicalMeasurement>>
        where TPhysiologicalMeasurement : PhysiologicalMeasurementInfo
    {
    }
}
