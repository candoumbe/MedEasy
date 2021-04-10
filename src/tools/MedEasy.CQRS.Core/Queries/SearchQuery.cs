using MedEasy.DTO.Search;
using System;
using MedEasy.DAL.Repositories;

namespace MedEasy.CQRS.Core.Queries
{
    /// <summary>
    /// Query to filter resources
    /// </summary>
    /// <typeparam name="T">Type of the resources to perform search onto</typeparam>
    //[JsonObject]
    public class SearchQuery<T> : IQuery<Guid, SearchQueryInfo<T>, Page<T>>
    {
        public Guid Id { get; }

        public SearchQueryInfo<T> Data { get; }

        /// <summary>
        /// Builds a new <see cref="SearchQuery{T}"/> instance.
        /// </summary>
        /// <param name="search">Data the query carries.</param>
        /// <exception cref="ArgumentNullException">if <paramref name="search"/> is <c>null</c></exception>
        public SearchQuery(SearchQueryInfo<T> search)
        {
            if (Equals(search, default))
            {
                throw new ArgumentNullException(nameof(search));
            }
            Id = Guid.NewGuid();
            Data = search;
        }

        public override string ToString() => this.Jsonify();

    }
}
