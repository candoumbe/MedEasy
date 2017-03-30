using MedEasy.DTO;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries;
using System;

namespace MedEasy.Handlers.Core.Patient.Queries
{
    /// <summary>
    /// Defines the contract for classes that can handle
    /// </summary>
    public interface IHandleGetOnePatientInfoByIdQuery : IHandleQueryAsync<Guid, Guid, PatientInfo, IWantOneResource<Guid, Guid, PatientInfo>>
    {
    }
}