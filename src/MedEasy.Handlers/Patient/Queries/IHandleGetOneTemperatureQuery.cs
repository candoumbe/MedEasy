using MedEasy.DTO;
using MedEasy.Handlers.Queries;
using MedEasy.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.Handlers.Patient.Queries
{
    public interface IHandleGetOneTemperatureQuery : IHandleQueryAsync<Guid, GetOnePhysiologicalMeasureInfo, TemperatureInfo, IWantOneResource<Guid, GetOnePhysiologicalMeasureInfo, TemperatureInfo>>
    {
    }
}
