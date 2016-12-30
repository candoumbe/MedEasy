using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedEasy.DAL.Repositories;
using MedEasy.Queries.Search;
using MedEasy.DAL.Interfaces;
using MedEasy.Data;
using System.Linq.Expressions;
using AutoMapper.QueryableExtensions;
using Microsoft.Extensions.Logging;

namespace MedEasy.Handlers
{
    /// <summary>
    /// Handlers for 
    /// </summary>
    public class HandleSearchQuery : IHandleSearchQuery
    {
        private readonly IExpressionBuilder _expressionBuilder;
        private readonly ILogger<HandleSearchQuery> _logger;
        private readonly IUnitOfWorkFactory _uowFactory;

        /// <summary>
        /// Creates a new <see cref="HandleSearchQuery"/> instance.
        /// </summary>
        /// <param name="uowFactory">Unit of work factory</param>
        /// <param name="expressionBuilder">Expression builder to map an entity type to result type</param>
        /// <exception cref="ArgumentNullException">if either <paramref name="uowFactory"/> or <paramref name="expressionBuilder"/> is <c>null</c>.</exception>
        public HandleSearchQuery(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder, ILogger<HandleSearchQuery> logger)
        {
            _uowFactory = uowFactory;
            _expressionBuilder = expressionBuilder;
            _logger = logger;
        }

        public async Task<IPagedResult<TResult>> Search<TEntity, TResult>(SearchQuery<TResult> searchQuery) where TEntity : class
        {
            using (var uow = _uowFactory.New())
            {

                _logger.LogInformation("Start searching");
                _logger.LogDebug($"Query : {searchQuery}");

                Expression<Func<TResult, bool>> filter = searchQuery.Data.Filter.ToExpression<TResult>();
                int page = searchQuery.Data.Page;
                int pageSize = searchQuery.Data.PageSize;
                Expression<Func<TEntity, TResult>> selector = _expressionBuilder.CreateMapExpression<TEntity, TResult>();
                IPagedResult<TResult> result = await uow.Repository<TEntity>()
                    .WhereAsync(selector, filter, Enumerable.Empty<OrderClause<TResult>>(), page, pageSize)
                    .ConfigureAwait(false);

                    

                return result;
            }
        }
    }
}
