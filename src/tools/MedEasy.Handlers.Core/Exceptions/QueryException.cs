using MedEasy.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MedEasy.Handlers.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when handling a query
    /// </summary>
    public class QueryException : Exception
    {
        /// <summary>
        /// Additional informations that caused the errors to be thrown
        /// </summary>
        public IEnumerable<ErrorInfo> Errors { get; }

        /// <summary>
        /// Builds a new <see cref="QueryException"/> that has the specified message
        /// </summary>
        /// <param name="message">Message of the exception</param>
        public QueryException(string message) : this(message, Enumerable.Empty<ErrorInfo>())
        {
        }

        public QueryException(string message, IEnumerable<ErrorInfo> errors) : base(message)
        {
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }
    }
}
