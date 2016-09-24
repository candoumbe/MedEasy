using MedEasy.DTO;
using MedEasy.Handlers.Queries;
using MedEasy.Queries;
using System;

namespace MedEasy.Handlers.Patient.Queries
{
    /// <summary>
    /// Defines the contract 
    /// </summary>
    public interface IHandleGetPatientDetailsByIdQuery : IHandleQueryAsync<Guid, int, PatientInfo, IQuery<Guid, int, PatientInfo>>
    {
    }
}