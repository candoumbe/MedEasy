using MedEasy.Commands;
using System;
using System.Threading.Tasks;
using MedEasy.Handlers.Exceptions;

namespace MedEasy.Handlers.Commands
{
    /// <summary>
    /// Defines the contract that any command handler must fullfil.
    /// </summary>
    /// <remarks>
    /// <see cref="RunAsync(TCommand)"/> returns <see cref="TOutput"/>
    /// </remarks>
    /// <typeparam name="TKey">Type of the key that uniquely identify a command</typeparam>
    /// <typeparam name="TOutput">Type of the data processing will output</typeparam>
    /// <typeparam name="TInput">Type of the data the command carries</typeparam>
    /// <typeparam name="TCommand">Type of the command</typeparam>
    public interface IRunCommandAsync<TKey, TInput, TOutput, TCommand> 
        where TCommand : ICommand<TKey, TInput>
        where TKey : IEquatable<TKey>

    {
        /// <summary>
        /// Runs the specified <paramref name="command"/>.
        /// </summary>
        /// <remarks>
        /// A good practice when implementing this method is to validate the <paramref name="command"/> before processing.
        /// </remarks>
        /// <param name="command">The command to run</param>
        /// <returns>Data resulting of the execution of the command</returns>
        /// <exception cref="CommandNotValidException{TKey}">if <paramref name="command"/> is not valid</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="command"/>is null</exception>
        Task<TOutput> RunAsync(TCommand command);

        
    }


    /// <summary>
    /// Defines the contract that any commandq handler must fullfil.
    /// </summary>
    /// <remarks>
    ///     Allows to handle commands that produce no output
    /// </remarks>
    /// <typeparam name="TKey">Type of the key that uniquely identify a command</typeparam>
    /// <typeparam name="TInput">Type of the data the command carries</typeparam>
    /// <typeparam name="TCommand">Type of the command</typeparam>
    public interface IRunCommandAsync<TKey, TInput, TCommand>
        where TCommand : ICommand<TKey, TInput>
        where TKey : IEquatable<TKey>


    {
        /// <summary>
        /// Process the specified <paramref name="command"/>.
        /// </summary>
        /// <remarks>
        /// A good practice when implementing this command is to validate it before processing.
        /// </remarks>
        /// <param name="command">The command to process</param>
        /// <returns>Data resulting of the execution of the command</returns>
        /// <exception cref="CommandNotValidException{TCommandId}">if <paramref name="command"/> is not valid</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="command"/>is null</exception>
        Task RunAsync(TCommand command);
    }
}
