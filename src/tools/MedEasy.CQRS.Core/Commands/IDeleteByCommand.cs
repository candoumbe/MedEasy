using System;

namespace MedEasy.CQRS.Core.Commands
{
    /// <summary>
    /// Command to delete a element
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    /// <typeparam name="TKey">type of the identifier</typeparam>
    /// <typeparam name="TBy">Type of data the command will carry</typeparam>
    public interface IDeleteByCommand<TKey, TBy> : ICommand<TKey, TBy>
        where TKey : IEquatable<TKey>
    {
    }
}
