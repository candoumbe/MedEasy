using AutoMapper.QueryableExtensions;
using FluentValidation;
using FluentValidation.Results;
using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Interfaces;
using MedEasy.Handlers.Core.Exceptions;
using MedEasy.Objects;
using Microsoft.Extensions.Logging;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using static FluentValidation.Severity;

namespace MedEasy.Handlers.Core.Queries
{
    /// <summary>
    /// Generic handler for queries that request one single resource.
    /// </summary>
    /// <typeparam name="TQueryId">Type of the identifier of queries</typeparam>
    /// <typeparam name="TEntity">Type of the entity to create</typeparam>
    /// <typeparam name="TResult">Type of the query execution result</typeparam>
    /// <typeparam name="TData">Type of <see cref="TQuery"/> data</typeparam>
    /// <typeparam name="TQuery">Type of the query</typeparam>
    /// <
    /// <typeparam name="TQueryValidator">Type of the validator of the <typeparamref name="TQuery"/> query</typeparam>
    public abstract class GenericGetOneByIdQueryHandler<TQueryId, TEntity, TData, TResult, TQuery, TQueryValidator> : OneResourceQueryHandlerBase<TQueryId, TEntity, TData, TResult, TQuery, TQueryValidator>
    where TQuery : IWantOne<TQueryId, TData, TResult>
    where TEntity : class, IEntity<int>
    where TQueryId : IEquatable<TQueryId>
    where TQueryValidator : class, IValidator<TQuery>
    {
        private readonly TQueryValidator _validator;

        protected ILogger<GenericGetOneByIdQueryHandler<TQueryId, TEntity, TData, TResult, TQuery, TQueryValidator>> Logger { get; }

        protected IExpressionBuilder ExpressionBuilder { get; }

        /// <summary>
        /// Builds a new <see cref="GenericGetOneByIdQueryHandler{TQueryId, TEntity, TData, TResult, TQuery, TQueryValidator}"/> instance
        /// </summary>
        /// <param name="validator">Validator to use to validate commands in <see cref="HandleAsync(TQuery)"/></param>
        /// <param name="logger">Logger</param>
        /// <param name="uowFactory">Factory to build instances of <see cref="IUnitOfWork"/></param>
        /// <param name="dataToEntityMapper">Function to convert commands data to entity</param>
        /// <param name="expressionBuilder">Container of expressions that will be used to convert <see cref="TEntity"/> to <see cref="TResult"/></param>
        protected GenericGetOneByIdQueryHandler(TQueryValidator validator,
            ILogger<GenericGetOneByIdQueryHandler<TQueryId, TEntity, TData, TResult, TQuery, TQueryValidator>> logger,
            IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder) : base(validator, uowFactory)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ExpressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
            _validator = validator;
        }

        /// <summary>
        /// Process the command.
        /// The command is validates prior to being processed
        /// </summary>
        /// <param name="query">command to run</param>
        /// <returns>The result of the command execution</returns>
        /// <exception cref="QueryNotValidException{TQueryId}">if  <paramref name="query"/> validation fails</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="query"/> is <c>null</c></exception>
        public override async ValueTask<Option<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken = default)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            Logger.LogInformation($"Start executing query : {query.Id}");
            Logger.LogTrace("Validating query");
            ValidationResult vr = await _validator.ValidateAsync(query, cancellationToken)
                .ConfigureAwait(false);
            
            if (vr.Errors.AtLeastOnce(error => error.Severity == Error))
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
                TData data = query.Data;

                Expression<Func<TEntity, TResult>> selector = ExpressionBuilder.GetMapExpression<TEntity, TResult>();
                Option<TResult> output = await uow.Repository<TEntity>().SingleOrDefaultAsync(selector, x => Equals(x.UUID, data));

                Logger.LogInformation($"Query {query.Id} processed successfully");

                return output;
            }
        }
    }

    /// <summary>
    /// Generic handler for queries that request one single resource.
    /// </summary>
    /// <typeparam name="TQueryId">Type of the identifier of queries</typeparam>
    /// <typeparam name="TEntity">Type of the entity to create</typeparam>
    /// <typeparam name="TResult">Type of the query execution résult</typeparam>
    /// <typeparam name="TQuery">Type of the query</typeparam>
    public abstract class GenericGetOneByIdQueryHandler<TQueryId, TEntity, TResult, TQuery> : GenericGetOneByIdQueryHandler<TQueryId, TEntity, Guid, TResult, TQuery, IValidator<TQuery>>
        where TQuery : IWantOne<TQueryId, Guid, TResult>
        where TEntity : class, IEntity<int>
        where TQueryId : IEquatable<TQueryId>
    {

        /// <summary>
        /// Builds a new <see cref="GenericGetOneByIdQueryHandler{TKey, TEntity, TData, TResult, TCommand}"/> instance
        /// </summary>
        /// <param name="validator">Validator to use to validate commands in <see cref="HandleAsync(TQuery)"/></param>
        /// <param name="logger">Logger</param>
        /// <param name="uowFactory">Factory to build instances of <see cref="IUnitOfWork"/></param>
        /// <param name="dataToEntityMapper">Function to convert commands data to entity</param>
        /// <param name="expressionBuilder">Container of expressions that will be used to convert <see cref="TEntity"/> to <see cref="TResult"/></param>
        protected GenericGetOneByIdQueryHandler(
            ILogger<GenericGetOneByIdQueryHandler<TQueryId, TEntity, TResult, TQuery>> logger,
            IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder) : base(Validator<TQuery>.Default, logger, uowFactory, expressionBuilder)
        {
        }
    }

}
