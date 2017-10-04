using MedEasy.Validators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedEasy.Handlers.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when the execution of a command will result in a data/business conflict.
    /// </summary>
    /// <typeparam name="TCommandId">Type of the id of the command that the exception will be thrown on validation failure</typeparam>
    public class CommandConflictException<TCommandId> : CommandException
    {
        /// <summary>
        /// Command that causes the exception to be thrown
        /// </summary>
        public TCommandId CommandId { get; }


        /// <summary>
        /// Builds a new <see cref="CommandConflictException{TCommandId}"/> instance
        /// </summary>
        /// <param name="commandId"><see cref="ICommmand.Id"/> that cause the exception to be thrown</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="commandId"/> is equals to default value of <see cref="TCommandId"/></exception>
        public CommandConflictException(TCommandId commandId) : base(string.Empty)
        {
            if (Equals(default, commandId))
            {
                throw new ArgumentOutOfRangeException(nameof(commandId), $"{nameof(commandId)} must not be default value");
            }
            CommandId = commandId;
        }
    }
}