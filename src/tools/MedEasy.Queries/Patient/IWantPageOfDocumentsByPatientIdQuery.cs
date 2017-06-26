using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.RestObjects;
using Optional;
using System;
using System.Collections.Generic;

namespace MedEasy.Queries.Patient
{
    /// <summary>
    /// Request a page of <see cref="DocumentMetadataInfo"/>s for a patient.
    /// </summary>
    public interface IWantPageOfDocumentsByPatientIdQuery : IWantResource<Guid, GetDocumentsByPatientIdInfo, Option<IPagedResult<DocumentMetadataInfo>>>
    {
    }
}
