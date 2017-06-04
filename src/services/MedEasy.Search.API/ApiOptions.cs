using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.Search.API
{
    /// <summary>
    /// Wraps application settings 
    /// </summary>
    public class ApiOptions
    {
        /// <summary>
        /// Default size of a page of result
        /// </summary>
        public int DefaultPageSize { get; set; }

        /// <summary>
        /// Maximum allowed page size 
        /// </summary>
        public int MaxPageSize { get; set; }
    }
}
