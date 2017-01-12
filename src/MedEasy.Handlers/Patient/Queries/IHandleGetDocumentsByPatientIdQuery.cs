using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.Handlers.Queries;
using MedEasy.Objects;
using MedEasy.Queries;
using MedEasy.Queries.Patient;
using System;
using System.Collections.Generic;

namespace MedEasy.Handlers.Patient.Queries
{
    /// <summary>
    /// Defines methods for handling requests that lookup for <see cref="DocumentInfo"/>s for a <see cref="Objects.Patient"/>
    /// </summary>
    /// <typeparam name="TPhysiologicalMeasurement">Type of the physiological measurement that will be handled</typeparam>
    public interface IHandleGetDocumentsByPatientIdQuery : IHandleQueryAsync<Guid, GetDocumentsByPatientIdInfo, IPagedResult<DocumentInfo>, IWantDocumentsByPatientIdQuery>
    {
    }
}
