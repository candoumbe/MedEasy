using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.RestObjects;
using System;
using System.Collections.Generic;

namespace MedEasy.Queries.Patient
{
    /// <summary>
    /// Request many <see cref="DocumentInfo"/>s of a patient.
    /// </summary>
    public interface IWantDocumentsByPatientIdQuery : IQuery<Guid, GetDocumentsByPatientIdInfo, IPagedResult<DocumentInfo>>
    {
    }
}
