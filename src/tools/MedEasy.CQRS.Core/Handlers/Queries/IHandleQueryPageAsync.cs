﻿using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;
using System;

namespace MedEasy.CQRS.Core.Handlers.Queries
{

    /// <summary>
    /// Defines methods of handler that can process queries that request a "page" of resources
    /// </summary>
    /// <typeparam name="TKey">Type of the key that identifies queries that this handler can execute</typeparam>
    /// <typeparam name="TData">Type of the data queries will carry</typeparam>
    /// <typeparam name="TResult">Type of the result of the execution of the query. Will be wrapped in a <see cref="Page{TResult}"/></typeparam>
    /// <typeparam name="TQuery">Type of queries this instance can handle</typeparam>
    public interface IHandleQueryPageAsync<TKey, TData, TResult, TQuery> : IHandleQueryAsync<TKey, TData, Page<TResult>, TQuery>
        where TKey : IEquatable<TKey>
        where TQuery : IWantPage<TKey, TData, TResult>
    {
    }

    /// <summary>
    /// Defines methods of handler that can process queries that request a "page" of resources
    /// </summary>
    /// <typeparam name="TKey">Type of the key that identifies queries that this handler can execute</typeparam>
    /// <typeparam name="TResult">Type of the result of the execution of the query. Will be wrapped in a <see cref="Task{TResult}"/></typeparam>
    /// <typeparam name="TQuery">Type of queries this instance can handle</typeparam>
    public interface IHandleQueryPageAsync<TKey, TResult, TQuery> : IHandleQueryPageAsync<TKey, PaginationConfiguration, TResult, TQuery>

        where TKey : IEquatable<TKey>
        where TQuery : IWantPage<TKey, PaginationConfiguration, TResult>
    {
    }
}