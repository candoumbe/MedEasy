﻿using Newtonsoft.Json;
using static Newtonsoft.Json.JsonConvert;

namespace MedEasy.RestObjects
{
    /// <summary>
    /// An instance of this class holds parameters of a "GET" query.
    /// </summary>
    /// <remarks>
    /// Both <see cref="PageSize"/> and <see cref="Page"/> returns <c>null</c> or positive integer
    /// </remarks>
    [JsonObject]
    public class PaginationConfiguration
    {
        private int _pageSize;

        private int _page;

        /// <summary>
        /// Size of a page if none provided
        /// </summary>
        public const int DefaultPageSize = 30;
        /// <summary>
        /// Max size of the page
        /// </summary>
        public const int MaxPageSize = 200;

        /// <summary>
        /// Builds a new <see cref="PaginationConfiguration"/> instance.
        /// </summary>
        /// <remarks>
        /// <see cref="Page"/> is set to <c>1</c> and <see cref="PageSize"/> is set to <see cref="DefaultPageSize"/> value
        /// </remarks>
        public PaginationConfiguration()
        {
            PageSize = DefaultPageSize;
            Page = 1;
        }

        /// <summary>
        /// Gets/Sets the maximum number of resource to get. 
        /// </summary>
        /// <remarks>
        /// Number of items a page can contain at most.
        /// </remarks>
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = value < 1 ? 1 : value;
            }
        }

        /// <summary>
        /// Gets/Sets the page of data to retrieve.
        /// </summary>
        /// <remarks>
        /// The page is set to <c>1</c> whenever the value is less than 1
        /// </remarks>
        public int Page
        {
            get
            {
                return _page;
            }
            set
            {
                _page = value < 1 ? 1 : value;
            }
        }


        public override string ToString() => SerializeObject(this);

    }
}