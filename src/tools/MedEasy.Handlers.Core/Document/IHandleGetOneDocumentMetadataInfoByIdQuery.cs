using System;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries;

namespace MedEasy.Handlers.Core.Document.Queries
{
    public interface IHandleGetOneDocumentMetadataInfoByIdQuery : IHandleQueryOneAsync<Guid, Guid, DocumentMetadataInfo, IWantOneResource<Guid, Guid, DocumentMetadataInfo>>
    {
    }
}
