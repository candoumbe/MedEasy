using MediatR;
using System;

namespace MedEasy.CQRS.Core.Commands
{
    /// <summary>
    /// <para>
    /// Defines the shape of a command in Command & Query Responsability Separation pattern.
    /// </para>
    /// <para>
    /// A command should be uniquely identified by its <see cref="Id"/> which means that 2 <see cref="ICommand{TKey, TData}"/> 
    /// instances with
    /// same <see cref="Id"/>s referes to the same command
    /// </para>
    /// </summary>
    /// <typeparam name="TKey">type of the command identifier</typeparam>
    /// <typeparam name="TData">type of data the command carries</typeparam>
    public interface ICommand<TKey, TData, TResult> : IRequest<TResult>
        where TKey : IEquatable<TKey> 
    {
        /// <summary>
        /// Id of the command. Should be unique to identify the command
        /// </summary>
        TKey Id { get; }

        /// <summary>
        /// Data the command carries
        /// </summary>
        TData Data { get; }
    }

    /// <summary>
    /// <para>
    /// Defines the shape of a command in Command & Query Responsability Separation pattern.
    /// </para>
    /// <para>
    /// A command should be uniquely identified by its <see cref="Id"/> which means that 2 <see cref="ICommand{TKey, TData}"/> 
    /// instances with
    /// same <see cref="Id"/>s referes to the same command
    /// </para>
    /// </summary>
    /// <remarks>
    /// An instance of this command returns nothing
    /// </remarks>
    /// <typeparam name="TKey">type of the command identifier</typeparam>
    /// <typeparam name="TData">type of data the command carries</typeparam>
    public interface ICommand<TKey, TData> : IRequest
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Id of the command. Should be unique to identify the command
        /// </summary>
        TKey Id { get; }

        /// <summary>
        /// Data the command carries
        /// </summary>
        TData Data { get; }
    }
}
