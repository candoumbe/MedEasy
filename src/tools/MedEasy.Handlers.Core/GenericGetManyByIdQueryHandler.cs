using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MedEasy.DAL.Interfaces;
using System.Collections.Generic;
using System;
using System.Linq.Expressions;
using AutoMapper.QueryableExtensions;
using MedEasy.Handlers.Core.Exceptions;
using System.Threading;

namespace MedEasy.Handlers.Core.Queries
{
    /// <summary>
    /// Generic handler for queries that request Many single resource.
    /// </summary>
    /// <typeparam name="TQueryId">Type of the identifier of queries</typeparam>
    /// <typeparam name="TEntity">Type of the entity to create</typeparam>
    /// <typeparam name="TResult">Type of the query execution result</typeparam>
    /// <typeparam name="TEntityId">Type of <see cref="TQuery"/> data</typeparam>
    /// <typeparam name="TQuery">Type of the query</typeparam>
    /// <
    /// <typeparam name="TQueryValidator">Type of the validator of the <typeparamref name="TQuery"/> query</typeparam>
    public abstract class GenericGetManyByIdQueryHandler<TQueryId, TEntity, TEntityId, TResult, TQuery, TQueryValidator> 
        : QueryHandlerBase<TQueryId, TEntity, Guid, IEnumerable<TResult>, TQuery, TQueryValidator>
    where TQuery : IWantManyResources<TQueryId, Guid, TResult>
    where TEntity : class, IEntity<TEntityId>
    where TQueryId : IEquatable<TQueryId>
    where TQueryValidator : class, IValidator<TQuery>
    {

        protected ILogger<GenericGetManyByIdQueryHandler<TQueryId, TEntity, TEntityId, TResult, TQuery, TQueryValidator>> Logger { get; set; }

        protected IExpressionBuilder ExpressionBuilder { get;  }

        /// <summary>
        /// Builds a new <see cref="GenericGetManyByIdQueryHandler{TQueryId, TEntity, TData, TResult, TQuery, TQueryValidator}"/> instance
        /// </summary>
        /// <param name="validator">Validator to use to validate commands in <see cref="HandleAsync(TQuery)"/></param>
        /// <param name="logger">Logger</param>
        /// <param name="uowFactory">Factory to build instances of <see cref="IUnitOfWork"/></param>
        /// <param name="dataToEntityMapper">Function to convert commands data to entity</param>
        /// <param name="expressionBuilder">Container of expressions that will be used to convert <see cref="TEntity"/> to <see cref="TResult"/></param>
        protected GenericGetManyByIdQueryHandler(TQueryValidator validator,
            ILogger<GenericGetManyByIdQueryHandler<TQueryId, TEntity, TEntityId, TResult, TQuery, TQueryValidator>> logger,
            IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder) : base(validator, uowFactory)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ExpressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));

        }

        /// <summary>
        /// Process the command.
        ///
        /// The command is validates prior to being processed
        /// </summary>
        /// <param name="query">command to run</param>
        /// <returns>The result of the command execution</returns>
        /// <exception cref="QueryNotValidException{TQueryId}">if  <paramref name="query"/> validation fails</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="query"/> is <c>null</c></exception>
        public override async ValueTask<IEnumerable<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken = default)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            Logger.LogInformation($"Start executing query : {query.Id}");
            Logger.LogTrace("Validating query");
            ValidationResult vr= await Validator.ValidateAsync(query, cancellationToken);
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
                Guid data = query.Data;

                Expression<Func<TEntity, TResult>> selector = ExpressionBuilder.GetMapExpression<TEntity, TResult>();
                IEnumerable<TResult> output = await uow.Repository<TEntity>()
                    .WhereAsync(selector, x => Equals(x.UUID, data), cancellationToken: cancellationToken);

                Logger.LogInformation($"Query {query.Id} processed successfully");

                return output;
            }
        }
    }

    

}
