using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.RestObjects;
using System;
using System.Collections.Generic;

namespace MedEasy.Queries.Patient
{
    /// <summary>
    /// Request one <see cref="DocumentMetadataInfo"/> by its <see cref="PatientInfo.Id"/> and <see cref="DocumentMetadataInfo.Id"/>.
    /// </summary>
    public interface IWantOneDocumentByPatientIdAndDocumentIdQuery : IWantOneResource<Guid, GetOneDocumentInfoByPatientIdAndDocumentIdInfo, DocumentMetadataInfo>
    {
    }
}
