using MedEasy.DTO;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries;
using System;

namespace MedEasy.Handlers.Core.Prescription.Queries
{
    /// <summary>
    /// Base interface for handlers of one <see cref="PrescriptionHeaderInfo"/>
    /// </summary>
    public interface IHandleGetOnePrescriptionHeaderQuery : IHandleQueryAsync<Guid, int, PrescriptionHeaderInfo, IWantOneResource<Guid, int, PrescriptionHeaderInfo>>
    {
    }
}
