using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MedEasy.Data;
using AutoMapper.QueryableExtensions;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace MedEasy.CQRS.Core.Handlers
{
    /// <summary>
    /// Generic handler for search queries 
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

        public async Task<Page<TResult>> Search<TEntity, TResult>(SearchQuery<TResult> searchQuery, CancellationToken cancellationToken = default) where TEntity : class
        {
            using (IUnitOfWork uow = _uowFactory.New())
            {

                _logger.LogInformation("Start searching");
                _logger.LogDebug($"Query : {searchQuery}");

                Expression<Func<TResult, bool>> filter = searchQuery.Data.Filter.ToExpression<TResult>();
                int page = searchQuery.Data.Page;
                int pageSize = searchQuery.Data.PageSize;
                IEnumerable<OrderClause<TResult>> sorts = searchQuery.Data.Sorts
                    .Select(x => OrderClause<TResult>.Create(x.Expression, x.Direction == Data.SortDirection.Ascending ? DAL.Repositories.SortDirection.Ascending : DAL.Repositories.SortDirection.Descending));
                Expression<Func<TEntity, TResult>> selector = _expressionBuilder.GetMapExpression<TEntity, TResult>();
                Page<TResult> result = await uow.Repository<TEntity>()
                    .WhereAsync(selector, filter, sorts, pageSize, page, cancellationToken)
                    .ConfigureAwait(false);



                return result;
            }
        }
    }
}
