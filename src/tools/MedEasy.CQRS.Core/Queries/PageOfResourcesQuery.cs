using System;
using System.Collections.Generic;
using System.Text;
using MedEasy.RestObjects;

namespace MedEasy.CQRS.Core.Queries
{
    public abstract class PageOfResourcesQuery<TQueryId, TResult> : IWantPageOf<TQueryId, TResult>
        where TQueryId : IEquatable<TQueryId>
    {
        public TQueryId Id { get; }

        public PaginationConfiguration Data { get; }

        /// <summary>
        /// Builds a new <see cref="PageOfResourcesQuery{TQueryId, TResult}"/> instance
        /// </summary>
        /// <param name="id"></param>
        /// <param name="configuration"></param>
        protected PageOfResourcesQuery(TQueryId id, PaginationConfiguration configuration)
        {
            Id = id;
            Data = configuration;
        }
    }
}
