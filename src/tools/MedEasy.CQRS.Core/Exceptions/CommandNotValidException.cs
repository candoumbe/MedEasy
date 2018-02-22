using System;
using System.Collections.Generic;

namespace MedEasy.CQRS.Core.Exceptions
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
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="commandId"/> is equals to default value of <see cref="TCommandId"/></exception>
        public CommandNotValidException(TCommandId commandId, IEnumerable<ErrorInfo> errors) : base(errors)
        {
            if (Equals(default(TCommandId), commandId))
            {
                throw new ArgumentOutOfRangeException(nameof(commandId), $"{nameof(commandId)} must not be default value");
            }
            CommandId = commandId;
        }
    }
}
