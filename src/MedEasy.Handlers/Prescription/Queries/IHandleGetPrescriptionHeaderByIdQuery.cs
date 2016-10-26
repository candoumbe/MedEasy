using MedEasy.DTO;
using MedEasy.Handlers.Queries;
using MedEasy.Queries;
using System;

namespace MedEasy.Handlers.Prescription.Queries
{
    /// <summary>
    /// Defines the contract for classes that can handle
    /// </summary>
    public interface IHandleGetPrescriptionHeaderQuery : IHandleQueryAsync<Guid, int, PrescriptionHeaderInfo, IWantOneResource<Guid, int, PrescriptionHeaderInfo>>
    {
    }
}