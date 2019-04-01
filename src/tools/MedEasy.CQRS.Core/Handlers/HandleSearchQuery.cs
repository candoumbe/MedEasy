using AutoMapper.QueryableExtensions;
using DataFilters.Expressions;
using DataFilters;
using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using static System.Linq.Expressions.ExpressionExtensions;
using System.Diagnostics;
using MedEasy.DTO.Search;

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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Page<TResult>> Search<TEntity, TResult>(SearchQuery<TResult> searchQuery, CancellationToken cancellationToken = default) where TEntity : class
        {
            SearchQueryInfo<TResult> data = searchQuery.Data;
            Debug.Assert(data.Sort != null, "Sort expression should have been provided");
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                Expression<Func<TResult, bool>> filter = data.Filter?.ToExpression<TResult>() ?? True<TResult>();
                int page = data.Page;
                int pageSize = data.PageSize;
                
                Expression<Func<TEntity, TResult>> selector = _expressionBuilder.GetMapExpression<TEntity, TResult>();

                return await uow.Repository<TEntity>()
                    .WhereAsync(selector, filter, data.Sort, pageSize, page, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
