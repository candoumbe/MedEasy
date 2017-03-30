using System;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using MedEasy.DAL.Interfaces;
using MedEasy.DTO;
using Microsoft.Extensions.Logging;
using MedEasy.Objects;
using System.Linq.Expressions;
using MedEasy.DAL.Repositories;
using static MedEasy.DAL.Repositories.SortDirection;
using System.Linq;
using MedEasy.Queries.Document;
using MedEasy.Handlers.Core.Document.Queries;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries;

namespace MedEasy.Handlers.Document.Queries
{
    public class HandleGetOneDocumentMetadataInfoByIdQuery : GenericGetOneByIdQueryHandler<Guid, DocumentMetadata, DocumentMetadataInfo, IWantOneResource<Guid, Guid, DocumentMetadataInfo>>, IHandleGetOneDocumentMetadataInfoByIdQuery
    {


        /// <summary>
        /// Builds a new <see cref="HandleGetOneDocumentMetadataInfoByIdQuery"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="logger"></param>
        /// <param name="expressionBuilder"></param>
        public HandleGetOneDocumentMetadataInfoByIdQuery(IUnitOfWorkFactory uowFactory, ILogger<HandleGetOneDocumentMetadataInfoByIdQuery> logger, IExpressionBuilder expressionBuilder)
            : base(logger, uowFactory, expressionBuilder)
        {
        }

    }
}
