using MedEasy.Data;
using System.Collections.Generic;
using static Newtonsoft.Json.JsonConvert;
using System.Linq;
using System;
using Newtonsoft.Json;

namespace MedEasy.DTO.Search
{
    /// <summary>
    /// Represents a request for a search on resources of type.
    /// </summary>
    /// <typeparam name="T">Type of resource the search query will be applied on</typeparam>
    [JsonObject]
    public class SearchQueryInfo<T> 
    {

        /// <summary>
        /// Defines how to sort the result of the search
        /// </summary>
        public IEnumerable<Sort> Sorts { get; set; }

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
        public IDataFilter Filter { get; set; }

        /// <summary>
        /// Builds a new <see cref="SearchQueryInfo{T}"/> instance.
        /// </summary>
        public SearchQueryInfo()
        {
            Sorts = Enumerable.Empty<Sort>();
        }


        public override string ToString()
            => $"{nameof(Filter)} : {SerializeObject(Filter)}," + Environment.NewLine +
            $"{nameof(Page)}: {Page}" + Environment.NewLine +
            $"{nameof(PageSize)}: {PageSize}" + Environment.NewLine +
            $"{nameof(Sorts)} : {string.Join(",", Sorts.Select(x => new { Expression = x.Expression.Body.ToString(), x.Direction }))}";


    }
}
