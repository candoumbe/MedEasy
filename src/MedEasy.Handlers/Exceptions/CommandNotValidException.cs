﻿using MedEasy.Validators;
using MedEasy.Validators.Exceptions;
using System.Collections.Generic;

namespace MedEasy.Handlers.Exceptions
{
    /// <summary>
    /// Exception thrown when a command is not valid
    /// </summary>
    /// <typeparam name="TCommandId">Type of the id of the command that the exception will be thrown on validation failure</typeparam>
    public class CommandNotValidException<TCommandId> : ValidationException
    {
        /// <summary>
        /// Command that causes the exception to be thrown
        /// </summary>
        public TCommandId CommandId { get; }

                
        /// <summary>
        /// Builds a new <see cref="CommandNotValidException{TCommandId}"/> instance
        /// </summary>
        /// <param name="commandId"><see cref="ICommmand.Id"/> that cause the exception to be thrown</param>
        /// <param name="errors">errors that causes the exception to be thrown</param>
        public CommandNotValidException(TCommandId commandId, IEnumerable<ErrorInfo> errors) : base(errors)
        {
            CommandId = commandId;
        }
    }
}