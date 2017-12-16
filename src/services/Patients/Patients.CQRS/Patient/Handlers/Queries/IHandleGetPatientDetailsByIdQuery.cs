using MedEasy.CQRS.Core.Queries;
using MedEasy.Handlers.Core.Queries;
using Patients.DTO;
using System;

namespace Patients.CQRS.Patient.Handlers.Queries
{
    /// <summary>
    /// Defines the contract for classes that can handle
    /// </summary>
    public interface IHandleGetOnePatientInfoByIdQuery : IHandleQueryOneAsync<Guid, Guid, PatientInfo, IWantOne<Guid, Guid, PatientInfo>>
    {
    }
}