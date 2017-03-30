using System;
using MedEasy.DTO;
using MedEasy.Queries;
using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;
using MedEasy.Handlers.Core.Queries;

namespace MedEasy.Handlers.Core.Document.Queries
{
    public interface IHandleGetManyDocumentsQuery : IHandleQueryAsync<Guid, PaginationConfiguration, IPagedResult<DocumentMetadataInfo>, IWantManyResources<Guid, DocumentMetadataInfo>>
    {
    }
}
