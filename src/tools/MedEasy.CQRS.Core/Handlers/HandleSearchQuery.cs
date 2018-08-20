using AutoMapper.QueryableExtensions;
using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MedEasy.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using static System.Linq.Expressions.ExpressionExtensions;

namespace MedEasy.CQRS.Core.Handlers
{
    /// <summary>
    /// Generic handler for search queries 
    /// </summary>
    public class HandleSearchQuery : IHandleSearchQuery
    {
        private readonly IExpressionBuilder _expressionBuilder;
        private readonly IUnitOfWorkFactory _uowFactory;

        /// <summary>
        /// Creates a new <see cref="HandleSearchQuery"/> instance.
        /// </summary>
        /// <param name="uowFactory">Unit of work factory</param>
        /// <param name="expressionBuilder">Expression builder to map an entity type to result type</param>
        /// <exception cref="ArgumentNullException">if either <paramref name="uowFactory"/> or <paramref name="expressionBuilder"/> is <c>null</c>.</exception>
        public HandleSearchQuery(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder)
        {
            _uowFactory = uowFactory;
            _expressionBuilder = expressionBuilder;
        }


        /// <summary>
        /// Performs a search query against <typeparamref name="TEntity"/> datastore.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="searchQuery"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<Page<TResult>> Search<TEntity, TResult>(SearchQuery<TResult> searchQuery, CancellationToken ct = default) where TEntity : class
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                Expression<Func<TResult, bool>> filter = searchQuery.Data.Filter?.ToExpression<TResult>() ?? True<TResult>();
                int page = searchQuery.Data.Page;
                int pageSize = searchQuery.Data.PageSize;
                IEnumerable<OrderClause<TResult>> sorts = searchQuery.Data.Sorts
                    .Select(x => OrderClause<TResult>.Create(x.Expression, x.Direction == Data.SortDirection.Ascending ? DAL.Repositories.SortDirection.Ascending : DAL.Repositories.SortDirection.Descending));
                Expression<Func<TEntity, TResult>> selector = _expressionBuilder.GetMapExpression<TEntity, TResult>();

                return await uow.Repository<TEntity>()
                    .WhereAsync(selector, filter, sorts, pageSize, page, ct)
                    .ConfigureAwait(false);
            }
        }
    }
}
