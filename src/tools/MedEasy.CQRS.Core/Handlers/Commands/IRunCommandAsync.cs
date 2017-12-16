using MedEasy.CQRS.Core.Exceptions;
using Optional;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MedEasy.CQRS.Core.Commands
{


    /// <summary>
    /// Defines the contract that any command handler must fullfil.
    /// </summary>
    /// <remarks>
    /// <see cref="RunAsync(TCommand, CancellationToken)"/> returns <see cref="TOutput"/>
    /// </remarks>
    /// <typeparam name="TCommandId">Type of the key that uniquely identify a command</typeparam>
    /// <typeparam name="TCommandData">Type of the data the command carries</typeparam>
    /// <typeparam name="TOutput">Type of the data processing will output</typeparam>
    /// <typeparam name="TCommand">Type of the command</typeparam>
    public interface IRunCommandAsync<TCommandId, in TCommandData, TOutput, in TCommand> 
        where TCommandId : IEquatable<TCommandId>
        where TCommand : ICommand<TCommandId, TCommandData, TOutput>
    {
        /// <summary>   
        /// Runs the specified <paramref name="command"/>.
        /// </summary>
        /// <remarks>
        /// A good practice when implementing this method is to validate the <paramref name="command"/> before processing.
        /// An optional <paramref name="cancellationToken"/> can be passed to abort processing the command
        /// </remarks>
        /// <param name="command">The command to run</param>
        /// <param name="cancellationToken">Notifies to cancel the execution of <paramref name="command"/></param>
        /// <returns>Data resulting of the execution of the command</returns>
        Task<Option<TOutput, CommandException>> RunAsync(TCommand command, CancellationToken cancellationToken = default);
    }


    /// <summary>
    /// Handler of commands that return no data.
    /// </summary>
    /// <remarks>
    ///     Allows to handle commands that produce no output
    /// </remarks>
    /// <typeparam name="TCommandId">Type of the key that uniquely identify a command</typeparam>
    /// <typeparam name="TCommandData">Type of the data the command carries</typeparam>
    /// <typeparam name="TCommand">Type of the command</typeparam>
    public interface IRunCommandAsync<TCommandId, in TCommandData, in TCommand> :  IRunCommandAsync<TCommandId, TCommandData, Nothing, TCommand>
        where TCommandId : IEquatable<TCommandId>
        where TCommand : ICommand<TCommandId, TCommandData, Nothing>
    {
    }





    

}
