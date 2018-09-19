using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedEasy.Mobile.Core.Services
{
    /// <summary>
    /// Exception thrown when a page was not found
    /// </summary>
    public class PageMissingException : Exception
    {
        /// <summary>
        /// Builds a new <see cref="PageMissingException"/> instance.
        /// </summary>
        /// <param name="page">Name of the missing page</param>
        public PageMissingException(string page) : base($"Page <{page}> not found")
        {
        }
    }
}
