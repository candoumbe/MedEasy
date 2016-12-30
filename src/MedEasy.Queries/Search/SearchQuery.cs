using MedEasy.DTO;
using System;
using System.Collections.Generic;
using static Newtonsoft.Json.JsonConvert;
using Newtonsoft.Json;

namespace MedEasy.Queries.Search
{
    /// <summary>
    /// Query to filter resources
    /// </summary>
    /// <typeparam name="T">Type of the resource to filter</typeparam>
    public class SearchQuery<T> : IQuery<Guid, SearchQueryInfo<T>, IEnumerable<T>>
    {
        public Guid Id { get; }

        public SearchQueryInfo<T> Data { get; }

        /// <summary>
        /// Builds a new <see cref="SearchQuery{T}"/> instance.
        /// </summary>
        /// <param name="search">Filter</param>
        /// <exception cref="ArgumentNullException">if <paramref name="search"/> is <c>null</c></exception>
        public SearchQuery(SearchQueryInfo<T> search)
        {
            Id = Guid.NewGuid();
            Data = search;
        }

        public override string ToString() => SerializeObject(this, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

    }
}
