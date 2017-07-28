using MedEasy.CQRS.Core;
using System;

namespace MedEasy.Commands
{
    /// <summary>
    /// <para>
    /// Defines the shape of a command in Command & Query Responsability Separation pattern.
    /// </para>
    /// <para>
    /// A command should be uniquely identified by its <see cref="Id"/> which means that 2 <see cref="ICommand{TKey, TData}"/> 
    /// instances with same <see cref="Id"/>s referes to the same command.
    /// </para>
    /// </summary>
    /// <typeparam name="TCommandId">type of the command identifier</typeparam>
    /// <typeparam name="TData">type of data the command carries</typeparam>
    public interface ICommand<TCommandId, TData> : ICommand<TCommandId, TData, Nothing>
        where TCommandId : IEquatable<TCommandId>
    {
    }

    /// <summary>
    /// <para>
    /// Defines the shape of a command in Command & Query Responsability Separation pattern.
    /// </para>
    /// <para>
    /// A command should be uniquely identified by its <see cref="Id"/> which means that 2 <see cref="ICommand{TKey, TData, TOutput}"/> 
    /// instances with
    /// same <see cref="Id"/>s referes to the same command
    /// </para>
    /// </summary>
    /// <typeparam name="TCommandId">type of the command identifier</typeparam>
    /// <typeparam name="TData">type of data the command carries</typeparam>
    public interface ICommand<TCommandId, TData, out TOutput> : IRequest<TCommandId, TOutput>
        where TCommandId : IEquatable<TCommandId>
    {

        /// <summary>
        /// Data the command carries
        /// </summary>
        TData Data { get; }

    }
}