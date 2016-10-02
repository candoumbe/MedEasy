using MedEasy.DTO;
using MedEasy.Handlers.Queries;
using MedEasy.Queries;
using System;

namespace MedEasy.Handlers.Patient.Queries
{
    /// <summary>
    /// Defines the contract for classes that can handle
    /// </summary>
    public interface IHandleGetOnePatientInfoByIdQuery : IHandleQueryAsync<Guid, int, PatientInfo, IWantOneResource<Guid, int, PatientInfo>>
    {
    }
}