using MedEasy.DTO;
using MedEasy.Handlers.Queries;
using MedEasy.Objects;
using MedEasy.Queries;
using System;

namespace MedEasy.Handlers.Prescription.Queries
{
    /// <summary>
    /// Base interface for handlers of one <see cref="PrescriptionHeaderInfo"/>
    /// </summary>
    public interface IHandleGetOnePrescriptionHeaderQuery : IHandleQueryAsync<Guid, int, PrescriptionHeaderInfo, IWantOneResource<Guid, int, PrescriptionHeaderInfo>>
    {
    }
}
