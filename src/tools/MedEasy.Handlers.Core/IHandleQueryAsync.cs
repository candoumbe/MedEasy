using MedEasy.Queries;
using Optional;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MedEasy.Handlers.Core.Queries
{
    /// <summary>
    /// Defines methods of handler that can process queries that request one resource
    /// </summary>
    /// <typeparam name="TKey">Type of the key that identifies queries that this handler can execute</typeparam>
    /// <typeparam name="TData">Type of the data queries will carry</typeparam>
    /// <typeparam name="TResult">Type of the result of the execution of the query. Will be wrapped in a <see cref="Task{TResult}"/></typeparam>
    /// <typeparam name="TQuery">Type of queries this instance can handle</typeparam>
    public interface IHandleQueryAsync<TKey, TData, TResult, TQuery> 
        where TKey : IEquatable<TKey>
        where TQuery : IQuery<TKey, TData, TResult>
    {

        /// <summary>
        /// Asynchronously handles the specified <see cref="query"/>
        /// </summary>
        /// <param name="query">the query to handle</param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="Task{TResult}"/></returns>
        ValueTask<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default(CancellationToken));
        
    }
}