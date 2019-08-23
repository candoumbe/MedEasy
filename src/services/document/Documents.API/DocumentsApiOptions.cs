using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Documents.API
{
    public class DocumentsApiOptions
    {
        /// <summary>
        /// Number of items to return when requiring a page of result
        /// </summary>
        public int DefaultPageSize { get; set; }

        /// <summary>
        /// Number of items the API can return in a single call at most
        /// </summary>
        public int MaxPageSize { get; set; }
    }
}
