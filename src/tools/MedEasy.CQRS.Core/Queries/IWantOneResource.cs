﻿namespace MedEasy.CQRS.Core.Queries
{
    using System;

    /// <summary>
    /// Defines the shape of a "query" to get one resource 
    /// <para>
    /// A query here stands for a request that is <strong>idempotent</strong>.
    /// </para>
    /// </summary>
    /// <typeparam name="TQueryId">Type of the identifier of the query</typeparam>
    /// <typeparam name="TData">Type of the data of the query</typeparam>
    /// <typeparam name="TResult">Type of the result of the query</typeparam>
    public interface IWantOneResource<TQueryId, TData, TResult> : IQuery<TQueryId, TData, TResult>
        where TQueryId : IEquatable<TQueryId>
    {

    }
}
