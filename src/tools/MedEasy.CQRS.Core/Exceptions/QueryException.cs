using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedEasy.CQRS.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when handling a query
    /// </summary>
    public class QueryException : Exception
    {
        /// <summary>
        /// Additional informations that caused the errors to be thrown
        /// </summary>
        public IEnumerable<ValidationFailure> Errors { get; }

        /// <summary>
        /// Builds a new <see cref="QueryException"/> that has the specified message
        /// </summary>
        /// <param name="message">Message of the exception</param>
        public QueryException(string message) : this(message, Enumerable.Empty<ValidationFailure>())
        {
        }

        /// <summary>
        /// Builds a new <see cref="QueryException"/> instance with its associated <see cref="ValidationFailure"/>s.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="errors"></param>
        public QueryException(string message, IEnumerable<ValidationFailure> errors) : base(message)
        {
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }
    }
}
