using System;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries;

namespace MedEasy.Handlers.Core.Document.Queries
{
    public interface IHandleGetOneDocumentInfoByIdQuery : IHandleQueryAsync<Guid, int, DocumentInfo, IWantOneResource<Guid, int, DocumentInfo>>
    {
    }
}
