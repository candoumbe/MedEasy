using MedEasy.Queries;
using System;
using System.Threading.Tasks;

namespace MedEasy.Handlers.Queries
{
    /// <summary>
    /// Defines methods of query handler
    /// </summary>
    /// <typeparam name="TKey">Type of the key that identifies queries that this handler can execute</typeparam>
    /// <typeparam name="TData">Type of the data queries will carry</typeparam>
    /// <typeparam name="TResult">Type of the result of the execution of the query. Will be wrapped in a <see cref="Task{TResult}"/></typeparam>
    /// <typeparam name="TQuery">Type of queries this instance can handle</typeparam>
    public interface IHandleQueryAsync<TKey, TData, TResult, in TQuery> 
        where TKey : IEquatable<TKey>
        where TQuery : IQuery<TKey, TData, TResult>
    {
        
        /// <summary>
        /// Asynchronously handles the specified <see cref="query"/>
        /// </summary>
        /// <param name="query">the query to handle</param>
        /// <returns><see cref="Task{TResult}"/></returns>
        Task<TResult> HandleAsync(TQuery query);
        
    }   
}