namespace MedEasy.CQRS.Core.Queries
{
    using MediatR;

    using System;

    /// <summary>
    /// Defines the shape of a "query" in CQRS design pattern.
    /// <para>
    /// A query here stands for a request that is <strong>idempotent</strong>.
    /// </para>
    /// </summary>
    /// <typeparam name="TKey">Type of the identifier of the query</typeparam>
    /// <typeparam name="TData">Type of the data of the query</typeparam>
    /// <typeparam name="TResult">Type of the result of the query</typeparam>
    public interface IQuery<TKey, TData, TResult> : IRequest<TResult>
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Id of the query
        /// </summary>
        TKey Id { get; }

        /// <summary>
        /// Data the query carries
        /// </summary>
        TData Data { get; }
    }
}
