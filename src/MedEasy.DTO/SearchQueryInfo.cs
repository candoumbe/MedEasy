using MedEasy.Data;
using System.Collections.Generic;
using static Newtonsoft.Json.JsonConvert;

namespace MedEasy.DTO
{
    /// <summary>
    /// Represents a request for a search on resources of type.
    /// </summary>
    /// <typeparam name="T">Type of resource the search query will be applied on</typeparam>
    public class SearchQueryInfo<T> : ISearchQueryInfo<T>
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


        public override string ToString() => SerializeObject(this);


    }
}
