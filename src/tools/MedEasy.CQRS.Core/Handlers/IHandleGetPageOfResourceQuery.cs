namespace MedEasy.CQRS.Core.Handlers
{
    using MedEasy.CQRS.Core.Queries;
    using MedEasy.DAL.Repositories;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Generic handlers for queries to get a page of result.
    /// </summary>
    public interface IHandleGetPageOfResourceQuery
    {
        /// <summary>
        /// Gets a page of result
        /// </summary>
        /// <param name="request">request</param>
        /// <param name="ct"></param>
        /// <returns><see cref="Page{T}"/> which holds the result</returns>
        /// <typeparam name="TQueryId">Type of the <paramref name="request"/> identifier</typeparam>
        /// <typeparam name="TEntity">Type of entity</typeparam>
        /// <typeparam name="TResult">Type of the result</typeparam>
        Task<Page<TResult>> GetPageAsync<TQueryId, TEntity, TResult>(IWantPageOf<TQueryId, TResult> request, CancellationToken ct = default)
            where TQueryId : IEquatable<TQueryId>
            where TEntity : class;
    }
}
