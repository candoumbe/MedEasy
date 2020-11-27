using System;
using System.Collections.Generic;
using System.Linq;

namespace MedEasy.CQRS.Core.Exceptions
{
    /// <summary>
    /// Base class for exceptions thrown when validating commands or queries
    /// </summary>
    public abstract class ValidationException : Exception
    {
        /// <summary>
        /// Errors that causes the exception to be thrown
        /// </summary>
        public IEnumerable<ErrorInfo> Errors { get; }

        protected ValidationException(IEnumerable<ErrorInfo> errors)
        {
            Errors = errors ?? Enumerable.Empty<ErrorInfo>();
        }
    }
}
