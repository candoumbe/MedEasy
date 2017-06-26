﻿using MedEasy.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MedEasy.Handlers.Core.Exceptions
{
    /// <summary>
    /// Thrown when a command cannot complete because the entity (or a related entity) was not found.
    /// </summary>
    public class CommandEntityNotFoundException : CommandException
    {
        /// <summary>
        /// Builds a new <see cref="CommandEntityNotFoundException"/> instance.
        /// </summary>
        public CommandEntityNotFoundException() : this(string.Empty)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">The message associated with the exception</param>
        public CommandEntityNotFoundException(string message) : this(message, Enumerable.Empty<ErrorInfo>())
        { }
        
        
        /// <summary>
        /// Builds a new <see cref="CommandEntityNotFoundException"/> instance.
        /// </summary>
        /// <param name="message">Message associated with the exception</param>
        public CommandEntityNotFoundException(string message, IEnumerable<ErrorInfo> errors) : base(message, errors)
        {

        }
    }
}
