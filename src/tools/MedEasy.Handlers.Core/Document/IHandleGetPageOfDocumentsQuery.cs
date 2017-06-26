using System;
using MedEasy.DTO;
using MedEasy.Queries;
using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;
using MedEasy.Handlers.Core.Queries;

namespace MedEasy.Handlers.Core.Document.Queries
{
    public interface IHandleGetPageOfDocumentsQuery : IHandleQueryPageAsync<Guid, PaginationConfiguration, DocumentMetadataInfo, IWantPageOfResources<Guid, DocumentMetadataInfo>>
    {
    }
}
