using System;
using System.Collections.Generic;
using System.Linq;

namespace MedEasy.Validators.Exceptions
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

        /// <summary>
        /// Builds a new <see cref="ValidationException"/> instance.
        /// </summary>
        /// <param name="errors"><see cref="ErrorInfo"/>s  that cause the exception to be thrown</param>
        protected ValidationException(IEnumerable<ErrorInfo> errors)
        {
            Errors = errors ?? Enumerable.Empty<ErrorInfo>();
        }
    }
}