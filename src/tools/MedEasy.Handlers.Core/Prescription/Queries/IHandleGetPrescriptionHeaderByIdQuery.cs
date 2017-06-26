using MedEasy.DTO;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries;
using System;

namespace MedEasy.Handlers.Core.Prescription.Queries
{
    /// <summary>
    /// Defines the contract for classes that can handle
    /// </summary>
    public interface IHandleGetPrescriptionHeaderQuery : IHandleQueryOneAsync<Guid, int, PrescriptionHeaderInfo, IWantOneResource<Guid, int, PrescriptionHeaderInfo>>
    {
    }
}