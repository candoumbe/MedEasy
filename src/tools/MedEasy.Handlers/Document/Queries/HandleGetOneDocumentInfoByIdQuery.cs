﻿using System;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using MedEasy.DAL.Interfaces;
using MedEasy.DTO;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using MedEasy.Handlers.Core.Document.Queries;
using MedEasy.Queries;
using System.Threading;
using Optional;

namespace MedEasy.Handlers.Document.Queries
{
    public class HandleGetOneDocumentInfoByIdQuery : IHandleGetOneDocumentInfoByIdQuery
    {
        public IUnitOfWorkFactory UowFactory { get; }
        public ILogger<HandleGetOneDocumentInfoByIdQuery> Logger { get; }
        public IExpressionBuilder ExpressionBuilder { get; }

        /// <summary>
        /// Builds a new <see cref="HandleGetOneDocumentInfoByIdQuery"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="logger"></param>
        /// <param name="expressionBuilder"></param>
        public HandleGetOneDocumentInfoByIdQuery(IUnitOfWorkFactory uowFactory, ILogger<HandleGetOneDocumentInfoByIdQuery> logger, IExpressionBuilder expressionBuilder)
        {
            UowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ExpressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder)); 
        }


        /// <summary>
        /// Process the command.
        /// The command is validates prior to being processed
        /// </summary>
        /// <param name="query">command to run</param>
        /// <returns>The result of the command execution</returns>
        /// <exception cref="QueryNotValidException{TQueryId}">if  <paramref name="query"/> validation fails</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="query"/> is <c>null</c></exception>
        public async ValueTask<Option<DocumentInfo>> HandleAsync(IWantOneResource<Guid, Guid, DocumentInfo> query, CancellationToken cancellationToken = default)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            Logger.LogInformation($"Start executing query : {query.Id}");
            Logger.LogTrace("Validating query");
//            IEnumerable<Task<ErrorInfo>> errorsTasks = Validator.Validate(query);
//            IEnumerable<ErrorInfo> errors = await Task.WhenAll(errorsTasks).ConfigureAwait(false);
//            if (errors.Any(item => item.Severity == Error))
//            {
//                Logger.LogTrace("validation failed", errors);
//#if DEBUG || TRACE
//                foreach (var error in errors)
//                {
//                    Logger.LogDebug($"{error.Key} - {error.Severity} : {error.Description}");
//                }
//#endif
//                throw new QueryNotValidException<Guid>(query.Id, errors);

//            }
            Logger.LogTrace("Query validation succeeded");

            using (IUnitOfWork uow = UowFactory.New())
            {
                Guid data = query.Data;

                Expression<Func<Objects.Document, DocumentInfo>> selector = ExpressionBuilder.GetMapExpression<Objects.Document, DocumentInfo>();
                Option<DocumentInfo> output = await uow.Repository<Objects.Document>()
                    .SingleOrDefaultAsync(selector, x => x.DocumentMetadata.UUID == data, cancellationToken);

                Logger.LogInformation($"Query {query.Id} processed successfully");

                return output;
            }
        }

    }
}
