﻿using System;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries;
using MedEasy.Queries.Document;

namespace MedEasy.Handlers.Core.Document.Queries
{
    public interface IHandleGetOneDocumentMetadataInfoByIdQuery : IHandleQueryAsync<Guid, int, DocumentMetadataInfo, IWantOneResource<Guid, int, DocumentMetadataInfo>>
    {
    }
}