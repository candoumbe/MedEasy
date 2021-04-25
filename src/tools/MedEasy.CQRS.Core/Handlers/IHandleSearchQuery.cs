using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Repositories;

using System.Threading;
using System.Threading.Tasks;

namespace MedEasy.CQRS.Core.Handlers
{
    public interface IHandleSearchQuery
    {
        /// <summary>
        /// Performs the search query
        /// </summary>
        /// <typeparam name="TEntity">Type of the resource to perform query on</typeparam>
        /// <typeparam name="TResult">Type of the result to perform query on</typeparam>
        /// <remarks>
        /// <typeparamref name="TEntity"/> shouuld be convertible to <typeparamref name="TResult"/.>
        /// </remarks>
        /// <param name="searchQuery"></param>
        /// <returns><see cref="IPagedResult{T}"/> which holds the result of the search</returns>
        Task<Page<TResult>> Search<TEntity, TResult>(SearchQuery<TResult> searchQuery, CancellationToken cancellationToken = default) where TEntity : class;
    }
}
