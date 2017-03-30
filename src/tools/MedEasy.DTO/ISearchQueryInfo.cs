using MedEasy.Data;
using System.Collections.Generic;

namespace MedEasy.DTO
{
    /// <summary>
    /// Generic search request
    /// </summary>
    /// <typeparam name="T">Type of data <see cref="Filter"/> will be apply on</typeparam>
    public interface ISearchQueryInfo<in T>
    {
        /// <summary>
        /// Sort order
        /// </summary>
        IEnumerable<Sort> Sorts { get; set; }

        /// <summary>
        /// Page of result
        /// </summary>
        int Page { set; get; }

        /// <summary>
        /// The page
        /// </summary>
        int PageSize { set; get; }
    }
}