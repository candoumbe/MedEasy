using MedEasy.CQRS.Core;
using System;

namespace MedEasy.Queries
{
    /// <summary>
    /// Defines the shape of a "query" in CQRS design pattern.
    /// <para>
    /// A query here stands for a request that is <strong>idempotent</strong>.
    /// </para>
    /// </summary>
    /// <typeparam name="TQueryId">Type of the identifier of the query</typeparam>
    /// <typeparam name="TData">Type of the data of the query</typeparam>
    /// <typeparam name="TResult">Type of the result of the query</typeparam>
    public interface IQuery<TQueryId, TData, out TResult> : IRequest<TQueryId, TResult>
        where TQueryId : IEquatable<TQueryId>
    {
        /// <summary>
        /// Data the query carries
        /// </summary>
        TData Data { get; }

    }
}
