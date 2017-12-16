using System;
using System.Collections.Generic;
using System.Text;

namespace MedEasy.API.Core.Controllers
{
    public abstract class ApiOptions
    {
        /// <summary>
        /// DefaultPageSize
        /// </summary>
        public int DefaultPageSize { get; set; }

        /// <summary>
        /// Number of items a page of result can contain at most
        /// </summary>
        public int MaxPageSize { get; set; }
    }
}
