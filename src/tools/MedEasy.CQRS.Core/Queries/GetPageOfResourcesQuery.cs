using System;
using MedEasy.RestObjects;

namespace MedEasy.CQRS.Core.Queries
{
    public abstract class GetPageOfResourcesQuery<TQueryId, TResult> : IWantPageOf<TQueryId, TResult>
        where TQueryId : IEquatable<TQueryId>
    {
        public TQueryId Id { get; }

        public PaginationConfiguration Data { get; }

        /// <summary>
        /// Builds a new <see cref="GetPageOfResourcesQuery{TQueryId, TResult}"/> instance
        /// </summary>
        /// <param name="id"></param>
        /// <param name="configuration"></param>
        /// <exception cref="ArgumentNullException">if <paramref name="configuration"/> is <c>null</c>.</exception>
        protected GetPageOfResourcesQuery(TQueryId id, PaginationConfiguration configuration)
        {
            Id = id;
            Data = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
    }
}
