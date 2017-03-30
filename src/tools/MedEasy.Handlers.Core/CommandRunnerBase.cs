using MedEasy.Validators;
using System;
using System.Threading.Tasks;
using MedEasy.Commands;

namespace MedEasy.Handlers.Core.Commands
{
    /// <summary>
    /// Base class that can be used to quickly create a command handler.
    /// </summary>
    /// <remarks>
    ///     Commands processed by <see cref="CommandRunnerBase{TKey, TInput, TOutput, TCommand}"/> outputs <see cref="TOutput"/> instances
    /// </remarks>
    /// <typeparam name="TKey">Type of the identifiar of the command</typeparam>
    /// <typeparam name="TInput">Type of data commands carry</typeparam>
    /// <typeparam name="TOutput">Type of commands' output</typeparam>
    /// <typeparam name="TCommand">Type of command instances the current instance handle</typeparam>
    public abstract class CommandRunnerBase<TKey, TInput, TOutput, TCommand> : IRunCommandAsync<TKey, TInput, TOutput, TCommand>
        where TCommand : ICommand<TKey, TInput>
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Validator that will validate the command
        /// </summary>
        public IValidate<TCommand> Validator { get; }

        /// <summary>
        /// Builds a new <see cref="CommandRunnerBase{TKey, TInput, TOutput, TCommand}"/>
        /// </summary>
        /// <param name="validator">validator that will be used to validate <see cref="HandleAsync(TCommand)"/> parameter</param>
        protected CommandRunnerBase(IValidate<TCommand> validator)
        {
            Validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public abstract Task<TOutput> RunAsync(TCommand command);
    }

    /// <summary>
    /// Base class for building commands that have no output when running
    /// </summary>
    /// <typeparam name="TKey">Type of the command identifier</typeparam>
    /// <typeparam name="TInput">Type of data the command carries</typeparam>
    /// <typeparam name="TCommand">Type of the commmand</typeparam>
    public abstract class CommandRunnerBase<TKey, TInput, TCommand> : IRunCommandAsync<TKey, TInput, TCommand>
        where TCommand : ICommand<TKey, TInput>
        where TKey : IEquatable<TKey>
    {

        public IValidate<TCommand> Validator { get; }

        /// <summary>
        /// Builds a new <see cref="CommandRunnerBase{TKey, TInput, TOutput, TCommand}"/>
        /// </summary>
        /// <param name="validator">validator that will be used to validate <see cref="HandleAsync(TCommand)"/> parameter</param>
        protected CommandRunnerBase(IValidate<TCommand> validator)
        {
            Validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public abstract Task<Nothing> RunAsync(TCommand command);
    }
}