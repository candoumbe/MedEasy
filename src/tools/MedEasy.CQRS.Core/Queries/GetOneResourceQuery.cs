using System;

namespace MedEasy.CQRS.Core.Queries
{
    /// <summary>
    /// Base class for building queries to get one resource.
    /// </summary>
    /// <typeparam name="TQueryId">Type of the queries identifier.</typeparam>
    /// <typeparam name="TData">Type of data the query will carries.</typeparam>
    /// <typeparam name="TResult">Type of result of the query's execution.</typeparam>
    public abstract class GetOneResourceQuery<TQueryId, TData, TResult> : IWantOneResource<TQueryId, TData, TResult>
        where TQueryId : IEquatable<TQueryId>
    {
        /// <summary>
        /// Query's ID.
        /// </summary>
        public TQueryId Id { get; }

        /// <summary>
        /// Data the query carries
        /// </summary>
        public TData Data { get; }

        /// <summary>
        /// Builds a new <see cref="PageOfResourcesQuery{TQueryId, TResult}"/> instance
        /// </summary>
        /// <param name="id">Query's id. Should be unique to easily track the query's execution</param>
        /// <param name="data">data the current instance carries.</param>
        protected GetOneResourceQuery(TQueryId id, TData data)
        {
            Id = id;
            Data = data;
        }
    }
}
