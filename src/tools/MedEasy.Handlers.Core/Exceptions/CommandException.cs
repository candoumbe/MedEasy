using MedEasy.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MedEasy.Handlers.Core.Exceptions
{
    /// <summary>
    /// Base exception for commands
    /// </summary>
    public class CommandException : Exception
    {
        
        /// <summary>
        /// Additional informations that caused the errors to be thrown
        /// </summary>
        public IEnumerable<ErrorInfo> Errors { get; }

        /// <summary>
        /// Builds a new <see cref="CommandException"/> that has the specified message
        /// </summary>
        /// <param name="message">Message of the exception</param>
        public CommandException(string message) : this(message, Enumerable.Empty<ErrorInfo>())
        { 
        }

        public CommandException(string message, IEnumerable<ErrorInfo> errors) : base(message)
        {
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }
    }
}
