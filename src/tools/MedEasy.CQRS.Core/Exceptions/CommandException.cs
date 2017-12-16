using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedEasy.CQRS.Core.Exceptions
{
    /// <summary>
    /// Base exception class for commands
    /// </summary>
    public class CommandException : Exception
    {
        
        /// <summary>
        /// Additional informations that caused the errors to be thrown
        /// </summary>
        public IEnumerable<ValidationFailure> Errors { get; }

        /// <summary>
        /// Builds a new <see cref="CommandException"/> that has the specified message
        /// </summary>
        /// <param name="message">Message of the exception</param>
        public CommandException(string message) : this(message, Enumerable.Empty<ValidationFailure>())
        { 
        }

        public CommandException(string message, IEnumerable<ValidationFailure> errors) : base(message)
        {
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }
    }
}
