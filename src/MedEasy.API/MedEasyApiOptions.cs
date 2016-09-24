using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.API
{
    /// <summary>
    /// Options of the MedEasy API
    /// </summary>
    public class MedEasyApiOptions
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
