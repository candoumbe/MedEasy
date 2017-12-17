using AutoMapper.QueryableExtensions;
using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MedEasy.CQRS.Core.Handlers.Queries
{
    /// <summary>
    /// Base class that can be used to quickly create a command handler.
    /// </summary>
    /// <remarks>
    ///     Queries processed by <see cref="PagedResourcesQueryHandlerBase{TKey, TEntity, TData, TOutput, TQuery, TQueryValidator}"/> outputs
    /// </remarks>
    /// <typeparam name="TKey">Type of the identifiar of the command</typeparam>
    /// <typeparam name="TResult">Type of the expected <see cref="Page{T}"/> returned by <see cref="IHandleQueryAsync{TKey, TData, TResult, TQuery}.HandleAsync(TQuery, CancellationToken)"/></typeparam>
    /// <typeparam name="TQuery">Type of the query</typeparam>
    /// <typeparam name="TEntity">Type of the entity store will be handled against</typeparam>
    public abstract class PagedResourcesQueryHandlerBase<TKey, TEntity, TResult, TQuery> : QueryHandlerBase<TKey, TEntity, PaginationConfiguration, Page<TResult>, TQuery>
        where TQuery : IWantPage<TKey, PaginationConfiguration, TResult>
        where TKey : IEquatable<TKey>
        where TEntity : class
    {

        protected IExpressionBuilder ExpressionBuilder { get; }

        /// <summary>
        /// Builds a new <see cref="PagedResourcesQueryHandlerBase{TKey, TEntity, TData, TOutput, TQuery, TQueryValidator}"/>
        /// </summary>
        /// <param name="uowFactory">Factory used for accessing <see cref="TEntity"/> instances</param>
        /// <param name="expressionBuilder">Builds expression to map <see cref="TEntity"/> to <see cref="TResult"/></param>
        /// <exception cref="ArgumentNullException">if <paramref name="expressionBuilder"/> is <c>null</c></exception>
        protected PagedResourcesQueryHandlerBase(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder)
            : base(uowFactory)
        {
            ExpressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
        }

    
        public override async ValueTask<Page<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken = default)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            using (IUnitOfWork uow = UowFactory.New())
            {
                PaginationConfiguration data = query.Data;

                Expression<Func<TEntity, TResult>> selector = ExpressionBuilder.GetMapExpression<TEntity, TResult>();
                Page<TResult> output = await uow.Repository<TEntity>()
                    .ReadPageAsync(selector, data.PageSize, data.Page, cancellationToken: cancellationToken);

                //Logger.LogInformation($"Query {query.Id} processed successfully");

                return output;
            }
        }
    }
}