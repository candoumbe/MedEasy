using System;
using System.Threading.Tasks;
using System.Threading;
using Optional;
using MedEasy.Handlers.Core.Exceptions;
using MedEasy.CQRS.Core;
using MedEasy.CQRS.Core.Commands;

namespace MedEasy.Handlers.Core.Commands
{
    /// <summary>
    /// Base class that can be used to quickly create a command handler.
    /// </summary>
    /// <remarks>
    ///     Commands processed by <see cref="CommandRunnerBase{TKey, TInput, TOutput, TCommand}"/> outputs <see cref="TOutput"/> instances
    /// </remarks>
    /// <typeparam name="TCommandId">Type of the identifiar of the command</typeparam>
    /// <typeparam name="TCommandData">Type of data commands carry</typeparam>
    /// <typeparam name="TOutput">Type of  output</typeparam>
    public abstract class CommandRunnerBase<TCommandId, TCommandData, TOutput, TCommand> : IRunCommandAsync<TCommandId, TCommandData, TOutput, TCommand>
        where TCommandId : IEquatable<TCommandId>
        where TCommand : ICommand<TCommandId, TCommandData, TOutput>
    {

        /// <summary>
        /// Runs the specified <paramref name="command"/>.
        /// </summary>
        /// <param name="command">Command to run.</param>
        /// <param name="cancellationToken">Send notification to stop processing <paramref name="command"/> </param>
        /// <returns><see cref="Option{TOutput, CommandException}"/> which holds the result of <paramref name="command"/>'s execution.</returns>
        public abstract Task<Option<TOutput, CommandException>> RunAsync(TCommand command, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Base class for building commands that have no output when running
    /// </summary>
    /// <typeparam name="TCommandID">Type of the command identifier</typeparam>
    /// <typeparam name="TInput">Type of data the command carries</typeparam>
    /// <typeparam name="TCommand">Type of command the class can handle.</typeparam>
    public abstract class CommandRunnerBase<TCommandId, TInput, TCommand> : CommandRunnerBase<TCommandId, TInput, Nothing, TCommand>
        where TCommandId : IEquatable<TCommandId>
        where TCommand : ICommand<TCommandId, TInput, Nothing>

    {
    }
}