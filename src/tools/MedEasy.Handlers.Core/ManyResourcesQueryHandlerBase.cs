using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Linq.Expressions;
using AutoMapper.QueryableExtensions;
using MedEasy.Handlers.Core.Exceptions;
using System.Threading;
using MedEasy.DAL.Interfaces;
using MedEasy.CQRS.Core.Queries;
using MedEasy.Objects;
using FluentValidation;
using FluentValidation.Results;
using static FluentValidation.Severity;

namespace MedEasy.Handlers.Core.Queries
{
    /// <summary>
    /// Generic handler for queries that request many resources.
    /// </summary>
    /// <typeparam name="TQueryId">Type of the identifier of queries</typeparam>
    /// <typeparam name="TEntity">Type of the entity to create</typeparam>
    /// <typeparam name="TEntityId">Type of the data of queries this handles will carry. This is also the type of the resource identifier</typeparam>
    /// <typeparam name="TResult">Type of the query execution result</typeparam>
    /// <typeparam name="TQuery">Type of the query</typeparam>
    /// <typeparam name="TQueryValidator">Type of the query validator</typeparam>
    public abstract class ManyResourcesQueryHandlerBase<TQueryId, TEntity, TEntityId, TResult, TQuery, TQueryValidator> : QueryHandlerBase<TQueryId, TEntity, Expression<Func<TEntity, bool>>, IEnumerable<TResult>, TQuery>
        where TQuery : IWantMany<TQueryId, Expression<Func<TEntity, bool>>, TResult>
        where TEntity : class, IEntity<TEntityId>
        where TQueryId : IEquatable<TQueryId>
        where TQueryValidator : class, IValidator<TQuery>

    {

        protected IValidator<TQuery> Validator { get; }

        /// <summary>
        /// Builds a new <see cref="ManyResourcesQueryHandlerBase{TQueryId, TEntity, TEntityId, TResult, TQuery, TQueryValidator}"/> instance
        /// </summary>
        /// <param name="validator">Validator to use to validate commands in <see cref="HandleAsync(TCommand)"/></param>
        /// <param name="logger">Logger</param>
        /// <param name="uowFactory">Factory to build instances of <see cref="IUnitOfWork"/></param>
        /// <param name="dataToEntityMapper">Function to convert commands data to entity</param>
        /// <param name="expressionBuilder">Container of expressions that will be used to convert <see cref="TEntity"/> to <see cref="TResult"/></param>
        protected ManyResourcesQueryHandlerBase(TQueryValidator validator, 
            ILogger<ManyResourcesQueryHandlerBase<TQueryId, TEntity, TEntityId, TResult, TQuery, TQueryValidator>> logger, 
            IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder
            )
            : base (uowFactory)
        {
            Validator = validator ?? throw new ArgumentNullException(nameof(validator));
            Logger = logger;
            ExpressionBuilder = expressionBuilder;
        }
        

        protected ILogger<ManyResourcesQueryHandlerBase<TQueryId, TEntity, TEntityId, TResult, TQuery, TQueryValidator>> Logger { get; }

        protected IExpressionBuilder ExpressionBuilder { get; }
        
        /// <summary>
        /// Executes the specified <see cref="query"/>.
        /// The query is validated prior to its execution.
        /// </summary>
        /// <param name="query">query to run</param>
        /// <returns>The result of the command execution</returns>
        /// <exception cref="QueryNotValidException{TQueryId}">if  <paramref name="query"/> validation fails</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="query"/> is <c>null</c></exception>
        public override async ValueTask<IEnumerable<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken = default)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            Logger.LogInformation($"Start executing query '{query.Id}' : {query}");
            Logger.LogTrace("Validating query");
            ValidationResult vr = await Validator.ValidateAsync(query, cancellationToken)
                .ConfigureAwait(false);
            if (vr.Errors.AtLeastOnce(item => item.Severity == Error))
            {
                Logger.LogTrace("validation failed", vr.Errors);
#if DEBUG || TRACE
                foreach (ValidationFailure error in vr.Errors)
                {
                    Logger.LogDebug($"{error.PropertyName} - {error.Severity} : {error.ErrorMessage}");
                }
#endif
                throw new QueryNotValidException<TQueryId>(query.Id, vr.Errors);

            }
            Logger.LogTrace("Query validation succeeded");

            using (IUnitOfWork uow = UowFactory.New())
            {
                Expression<Func<TEntity, TResult>> selector = ExpressionBuilder.GetMapExpression<TEntity, TResult>();
                IEnumerable<TResult> entities = await uow.Repository<TEntity>()
                    .WhereAsync(selector, query.Data, cancellationToken);

                if (Logger.IsEnabled(LogLevel.Trace))
                {
                    int resultCount = entities.Count();
                    Logger.LogTrace($"{resultCount} result{(resultCount > 1 ? "s": string.Empty)} found");

                }
                Logger.LogInformation($"Query <{query.Id}> handled successfully");

                return entities;
            }
        }
    }


}
