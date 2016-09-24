using System;

namespace MedEasy.Commands
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
    /// <typeparam name="TBy">type of the identifier</typeparam>
    /// <typeparam name="TKey">type of data the command will carry</typeparam>
    public interface IDeleteByCommand<TKey, TBy> : ICommand<TKey, TBy>
        where TKey : IEquatable<TKey>
    {
        
    }
}