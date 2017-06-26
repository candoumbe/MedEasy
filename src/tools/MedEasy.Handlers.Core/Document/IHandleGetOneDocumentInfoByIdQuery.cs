using System;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries;

namespace MedEasy.Handlers.Core.Document.Queries
{
    public interface IHandleGetOneDocumentInfoByIdQuery : IHandleQueryOneAsync<Guid, Guid, DocumentInfo, IWantOneResource<Guid, Guid, DocumentInfo>>
    {
    }
}
