using System;
using AutoMapper.QueryableExtensions;
using MedEasy.DAL.Interfaces;
using MedEasy.DTO;
using Microsoft.Extensions.Logging;
using MedEasy.Objects;
using MedEasy.Handlers.Core.Document.Queries;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries;

namespace MedEasy.Handlers.Document.Queries
{
    public class HandleGetPageOfDocumentMetadataInfosQuery : PagedResourcesQueryHandlerBase<Guid, DocumentMetadata, DocumentMetadataInfo, IWantPageOfResources<Guid, DocumentMetadataInfo>>,  IHandleGetPageOfDocumentsQuery
    {
        
        /// <summary>
        /// Builds a new <see cref="HandleGetPageOfDocumentMetadataInfosQuery"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="logger"></param>
        /// <param name="expressionBuilder"></param>
        public HandleGetPageOfDocumentMetadataInfosQuery(IUnitOfWorkFactory uowFactory, ILogger<HandleGetPageOfDocumentMetadataInfosQuery> logger, IExpressionBuilder expressionBuilder)
            : base(uowFactory, expressionBuilder)
        {
        }

    }
}
