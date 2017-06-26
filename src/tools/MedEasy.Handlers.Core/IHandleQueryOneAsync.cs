using MedEasy.Queries;
using Optional;
using System;

namespace MedEasy.Handlers.Core.Queries
{
    /// <summary>
    /// Defines methods of handler that can process queries that request one resource
    /// </summary>
    /// <typeparam name="TKey">Type of the key that identifies queries that this handler can execute</typeparam>
    /// <typeparam name="TData">Type of the data queries will carry</typeparam>
    /// <typeparam name="TResult">Type of the result of the execution of the query. Will be wrapped in a <see cref="Task{TResult}"/></typeparam>
    /// <typeparam name="TQuery">Type of queries this instance can handle</typeparam>
    public interface IHandleQueryOneAsync<TKey, TData, TResult, 
        TQuery>  : IHandleQueryAsync<TKey, TData, Option<TResult>, TQuery>
        where TKey : IEquatable<TKey>
        where TQuery : IWantOneResource<TKey, TData, TResult>
    {
    }
}