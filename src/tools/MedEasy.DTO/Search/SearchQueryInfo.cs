using DataFilters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedEasy.DTO.Search
{
    /// <summary>
    /// Represents a request for a search on resources of type.
    /// </summary>
    /// <typeparam name="T">Type of resource the search query will be applied on</typeparam>
    //[JsonObject]
    public class SearchQueryInfo<T>
    {
        /// <summary>
        /// Defines how to sort the result of the search
        /// </summary>
        public ISort<T> Sort { get; set; }

        /// <summary>
        /// Page of result to see
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Filter to apply when searching for result
        /// </summary>
        public IFilter Filter { get; set; }


        public override string ToString() => this.Stringify();
    }
}
