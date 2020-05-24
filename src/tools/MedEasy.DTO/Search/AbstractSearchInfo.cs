using Forms;

using MedEasy.RestObjects;

namespace MedEasy.DTO.Search
{
    /// <summary>
    /// Base class
    /// </summary>
    /// <typeparam name="T">Type of the searched resources.</typeparam>
    public abstract class AbstractSearchInfo<T>
    {
        public const string SortPattern = @"^\s*(-|\+)?(([A-Za-z])\w*)+(\s*,\s*((-|\+)?(([A-Za-z])\w*)+)\s*)*$";
        public const char SortSeparator = ',';

        /// <summary>
        /// Index of the page of result to read.
        /// </summary>
        /// <remarks>
        /// The first page 
        /// </remarks>
        [FormField(Min = 1, Description = "Index of a page of results")]
        public int Page { get; set; }

        /// <summary>
        /// Size of a page 
        /// </summary>
        [FormField(Min = 1, Description = "Number of items per page")]
        public int PageSize { get; set; }

        /// <summary>
        /// Sorts
        /// </summary>
        [FormField(Pattern = SortPattern)]
        public string Sort { get; set; }

        protected AbstractSearchInfo()
        {
            Page = 1;
            PageSize = PaginationConfiguration.DefaultPageSize;
        }
    }
}
