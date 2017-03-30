using System.Threading.Tasks;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using MedEasy.DAL.Interfaces;
using System.Collections.Generic;
using System.Linq;
using static MedEasy.Validators.ErrorLevel;
using System;
using MedEasy.Objects;
using MedEasy.Queries;
using System.Linq.Expressions;
using AutoMapper.QueryableExtensions;
using MedEasy.Handlers.Core.Exceptions;

namespace MedEasy.Handlers.Core.Queries
{

    /// <summary>
    /// Generic handler for queries that request one single resource.
    /// </summary>
    /// <typeparam name="TQueryId">Type of the identifier of queries</typeparam>
    /// <typeparam name="TEntity">Type of the entity to create</typeparam>
    /// <typeparam name="TResult">Type of the query execution résult</typeparam>
    /// <typeparam name="TQuery">Type of the query</typeparam>
    public abstract class GenericGetOneByIdQueryHandler<TQueryId, TEntity, TResult, TQuery> : GenericGetOneByIdQueryHandler<TQueryId, TEntity, Guid, TResult, TQuery, IValidate<TQuery>>
        where TQuery : IQuery<TQueryId, Guid, TResult>
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
    public abstract class GenericGetOneByIdQueryHandler<TQueryId, TEntity, TData, TResult, TQuery, TQueryValidator> : QueryHandlerBase<TQueryId, TEntity,  TData, TResult, TQuery, TQueryValidator>
    where TQuery : IQuery<TQueryId, TData, TResult>
    where TEntity : class, IEntity<int>
    where TQueryId : IEquatable<TQueryId>
    where TQueryValidator : class, IValidate<TQuery>
    {
        private readonly TQueryValidator _validator;

        /// <summary>
        /// Builds a new <see cref="GenericGetOneByIdQueryHandler{TKey, TEntity, TData, TResult, TCommand}"/> instance
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


        protected ILogger<GenericGetOneByIdQueryHandler<TQueryId, TEntity, TData, TResult, TQuery, TQueryValidator>> Logger { get; }

        protected IExpressionBuilder ExpressionBuilder { get; }

        /// <summary>
        /// Process the command.
        /// The command is validates prior to being processed
        /// </summary>
        /// <param name="query">command to run</param>
        /// <returns>The result of the command execution</returns>
        /// <exception cref="QueryNotValidException{TQueryId}">if  <paramref name="query"/> validation fails</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="query"/> is <c>null</c></exception>
        public override async Task<TResult> HandleAsync(TQuery query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            Logger.LogInformation($"Start executing query : {query.Id}");
            Logger.LogTrace("Validating query");
            IEnumerable<Task<ErrorInfo>> errorsTasks = _validator.Validate(query);
            IEnumerable<ErrorInfo> errors = await Task.WhenAll(errorsTasks).ConfigureAwait(false);
            if (errors.Any(item => item.Severity == Error))
            {
                Logger.LogTrace("validation failed", errors);
#if DEBUG || TRACE
                foreach (ErrorInfo error in errors)
                {
                    Logger.LogDebug($"{error.Key} - {error.Severity} : {error.Description}");
                }
#endif
                throw new QueryNotValidException<TQueryId>(query.Id, errors);

            }
            Logger.LogTrace("Query validation succeeded");

            using (IUnitOfWork uow = UowFactory.New())
            {
                TData data = query.Data;

                Expression<Func<TEntity, TResult>> selector = ExpressionBuilder.CreateMapExpression<TEntity, TResult>();
                TResult output = await uow.Repository<TEntity>().SingleOrDefaultAsync(selector, x => Equals(x.UUID, data));

                Logger.LogInformation($"Query {query.Id} processed successfully");

                return output;
            }
        }
    }
}
