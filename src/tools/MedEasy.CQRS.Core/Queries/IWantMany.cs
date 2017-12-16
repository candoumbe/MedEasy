using System;
using System.Collections.Generic;

namespace MedEasy.CQRS.Core.Queries
{
    /// <summary>
    /// Interfaces for queries that request a "collection" of resources
    /// <para>
    /// A query here stands for a request that is <strong>idempotent</strong>.
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">Type of item the result collection will output.</typeparam>
    /// <typeparam name="TData">Type of data queries will carry.</typeparam>
    /// <typeparam name="TQueryId">Type of the query identifier.</typeparam>
    public interface IWantMany<TQueryId, TData, TResult> : IWant<TQueryId, TData, IEnumerable<TResult>>
        where TQueryId : IEquatable<TQueryId>
    {

    }
}
