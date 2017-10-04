using MedEasy.Validators;
using System;
using System.Threading.Tasks;
using MedEasy.DAL.Interfaces;
using MedEasy.Queries;
using System.Threading;
using FluentValidation;

namespace MedEasy.Handlers.Core.Queries
{

    /// <summary>
    /// Base class that can be used to quickly create a command handler.
    /// </summary>
    /// <remarks>
    ///     Queries processed by <see cref="QueryHandlerBase{TKey, TEntity, TData, TOutput, TQuery}"/> outputs
    /// </remarks>
    /// <typeparam name="TKey">Type of the identifiar of the command</typeparam>
    /// <typeparam name="TOutput">Type of the expected result of the <see cref="IExecuteQueryAsync{TKey, TResult, TQuery}"/></typeparam>
    /// <typeparam name="TQuery">Type of the query</typeparam>
    /// <typeparam name="TData">Type of data the query carries</typeparam>
    /// <typeparam name="TEntity">Type of the entity store will be handled against</typeparam>
    public abstract class QueryHandlerBase<TKey, TEntity, TData, TOutput, TQuery> : IHandleQueryAsync<TKey, TData, TOutput, TQuery>
        where TQuery : IQuery<TKey, TData, TOutput>
        where TKey : IEquatable<TKey>
    {
        

        protected IUnitOfWorkFactory UowFactory { get; }

        /// <summary>
        /// Builds a new <see cref="QueryHandlerBase{TKey, TEntity, TData, TOutput, TQuery}"/>
        /// </summary>
        /// <param name="validator">validator that will be used to validate <see cref="HandleAsync(TQuery)"/> parameter</param>
        /// <param name="uowFactory">Factory used for accessing <see cref="TEntity"/> instances</param>
        protected QueryHandlerBase(IUnitOfWorkFactory uowFactory)
        {
            UowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));

        }

        /// <summary>
        /// Handles <paramref name="query"/>.
        /// </summary>
        /// <param name="query">The query to process</param>
        /// <param name="cancellationToken">Notifies to cancel query execution</param>
        /// <returns></returns>
        public abstract ValueTask<TOutput> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Base class that can be used to quickly create a command handler.
    /// </summary>
    /// <remarks>
    ///     Queries processed by <see cref="QueryHandlerBase{TKey, TEntity, TData, TOutput, TQuery, TQueryValidator}"/> outputs
    /// </remarks>
    /// <typeparam name="TKey">Type of the identifiar of the command</typeparam>
    /// <typeparam name="TOutput">Type of the expected result of the <see cref="IQuery{TKey, TData, TResult}"/></typeparam>
    /// <typeparam name="TQuery">Type of the query</typeparam>
    /// <typeparam name="TData">Type of data the query carries</typeparam>
    /// <typeparam name="TEntity">Type of the entity store will be handled against</typeparam>
    public abstract class QueryHandlerBase<TKey, TEntity, TData, TOutput, TQuery, TQueryValidator> : QueryHandlerBase<TKey, TEntity, TData, TOutput, TQuery>
        where TQuery : IQuery<TKey, TData, TOutput>
        where TKey : IEquatable<TKey>
        where TQueryValidator : class, IValidator<TQuery>
    {
        /// <summary>
        /// Query validator
        /// </summary>
        protected TQueryValidator Validator { get; }

        /// <summary>
        /// Builds a new <see cref="QueryHandlerBase{TKey, TEntity, TData, TOutput, TQuery}"/>
        /// </summary>
        /// <param name="validator">validator that will be used to validate <see cref="HandleAsync(TQuery)"/> parameter</param>
        /// <param name="uowFactory">Factory used for accessing <see cref="TEntity"/> instances</param>
        protected QueryHandlerBase(TQueryValidator validator, IUnitOfWorkFactory uowFactory) : base(uowFactory)
        {
            Validator = validator ?? throw new ArgumentNullException(nameof(validator));
            
        }

    }


}