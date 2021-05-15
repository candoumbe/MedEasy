namespace Agenda.Models.v1.Search
{
    using MedEasy.Attributes;
    using MedEasy.RestObjects;

    public abstract class AbstractSearchModel<T>
    {
        public const string SortPattern = @"^\s*(-|\+)?(([A-Za-z])\w*)+(\s*,\s*((-|\+)?(([A-Za-z])\w*)+)\s*)*$";
        public const char SortSeparator = ',';


        /// <summary>
        /// 1-based index of the page of result to display
        /// </summary>
        [FormField(Min = 1, Description = "Index of a page of results")]
        [Minimum(1)]
        public int Page { get; set; }

        /// <summary>
        /// Number of elements a page of results
        /// </summary>
        /// <remarks>
        /// This is just
        /// </remarks>
        [FormField(Min = 1, Description = "Index of a page of results")]
        [Minimum(1)]
        public int PageSize { get; set; }

        /// <summary>
        /// Sorts
        /// </summary>
        [FormField(Pattern = SortPattern)]
        public string Sort { get; set; }

        protected AbstractSearchModel()
        {
            Page = 1;
            PageSize = PaginationConfiguration.DefaultPageSize;
        }
    }
}
