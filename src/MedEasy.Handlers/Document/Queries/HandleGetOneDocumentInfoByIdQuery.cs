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
using MedEasy.Validators;
using System.Collections.Generic;
using MedEasy.Handlers.Core.Exceptions;

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
            if (uowFactory == null)
            {
                throw new ArgumentNullException(nameof(uowFactory));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (expressionBuilder == null)
            {
                throw new ArgumentNullException(nameof(expressionBuilder));
            }

            UowFactory = uowFactory;
            Logger = logger;
            ExpressionBuilder = expressionBuilder; 
        }


        /// <summary>
        /// Process the command.
        /// The command is validates prior to being processed
        /// </summary>
        /// <param name="query">command to run</param>
        /// <returns>The result of the command execution</returns>
        /// <exception cref="QueryNotValidException{TQueryId}">if  <paramref name="query"/> validation fails</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="query"/> is <c>null</c></exception>
        public async Task<DocumentInfo> HandleAsync(IWantOneResource<Guid, int, DocumentInfo> query)
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

            using (var uow = UowFactory.New())
            {
                int data = query.Data;

                Expression<Func<Objects.Document, DocumentInfo>> selector = ExpressionBuilder.CreateMapExpression<Objects.Document, DocumentInfo>();
                DocumentInfo output = await uow.Repository<Objects.Document>().SingleOrDefaultAsync(selector, x => x.DocumentMetadataId == data);

                Logger.LogInformation($"Query {query.Id} processed successfully");

                return output;
            }
        }

    }
}
