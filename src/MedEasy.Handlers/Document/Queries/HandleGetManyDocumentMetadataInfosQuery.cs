using System;
using AutoMapper.QueryableExtensions;
using MedEasy.DAL.Interfaces;
using MedEasy.DTO;
using Microsoft.Extensions.Logging;
using MedEasy.Objects;
using MedEasy.Queries.Document;
using MedEasy.Handlers.Core.Document.Queries;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries;

namespace MedEasy.Handlers.Document.Queries
{
    public class HandleGetManyDocumentMetadataInfosQuery : GenericGetManyQueryHandler<Guid, DocumentMetadata, int, DocumentMetadataInfo, IWantManyResources<Guid, DocumentMetadataInfo>>,  IHandleGetManyDocumentsQuery
    {
        
        /// <summary>
        /// Builds a new <see cref="HandleGetManyDocumentMetadataInfosQuery"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="logger"></param>
        /// <param name="expressionBuilder"></param>
        public HandleGetManyDocumentMetadataInfosQuery(IUnitOfWorkFactory uowFactory, ILogger<HandleGetManyDocumentMetadataInfosQuery> logger, IExpressionBuilder expressionBuilder)
            : base(logger, uowFactory, expressionBuilder)
        {
        }

    }
}
