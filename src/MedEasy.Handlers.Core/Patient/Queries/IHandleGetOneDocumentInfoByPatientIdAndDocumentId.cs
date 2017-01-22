using MedEasy.DTO;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries.Patient;
using System;

namespace MedEasy.Handlers.Core.Patient.Queries
{

    /// <summary>
    /// Gets one <see cref="DocumentMetadataInfo"/> by its <see cref="DocumentMetadataInfo.PatientId"/> and its <see cref="DocumentMetadataInfo.Id"/>
    /// </summary>
    public interface IHandleGetOneDocumentInfoByPatientIdAndDocumentId : IHandleQueryAsync<Guid, GetOneDocumentInfoByPatientIdAndDocumentIdInfo, DocumentMetadataInfo, IWantOneDocumentByPatientIdAndDocumentIdQuery>
    {
    }
}
