using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries.Patient;
using System;

namespace MedEasy.Handlers.Core.Patient.Queries
{
    /// <summary>
    /// Defines methods for handling requests that lookup for <see cref="DocumentMetadataInfo"/>s for a <see cref="Objects.Patient"/>
    /// </summary>
    /// <typeparam name="TPhysiologicalMeasurement">Type of the physiological measurement that will be handled</typeparam>
    public interface IHandleGetDocumentsByPatientIdQuery : IHandleQueryAsync<Guid, GetDocumentsByPatientIdInfo, IPagedResult<DocumentMetadataInfo>, IWantDocumentsByPatientIdQuery>
    {
    }
}
