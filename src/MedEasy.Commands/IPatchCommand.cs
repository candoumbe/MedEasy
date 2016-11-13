using System;
using MedEasy.DTO;

namespace MedEasy.Commands
{
    /// <summary>
    /// Command to partially update a resource
    /// </summary>
    /// <remarks>
    /// This command embed a set of <see cref="ChangeInfo"/> that must be applied atomically.
    /// </remarks>
    /// <typeparam name="TKey">type of the command key</typeparam>
    /// <typeparam name="TResourceId">type of the identifier of resource to patch</typeparam>
    /// <typeparam name="TData">type of the data the command carries</typeparam>
    public interface IPatchCommand<TKey, TResourceId, TData> : ICommand<TKey, TData>
        where TKey : IEquatable<TKey>
        where TData : IPatchInfo<TResourceId>
    {
    }


    /// <summary>
    /// Command to partially update a resource.
    /// </summary>
    /// <remarks>
    /// This command embed a set of <see cref="ChangeInfo"/> that must be applied atomically.
    /// </remarks>
    /// <typeparam name="TResourceId">type of the identifier of the resource to patch</typeparam>
    /// <typeparam name="TData">type of the data the command carries</typeparam>
    public interface IPatchCommand<TResourceId, TData> : IPatchCommand<Guid, TResourceId, TData>
        where TData : IPatchInfo<TResourceId>
    {

    }


    /// <summary>
    /// Command to partially update a resource.
    /// </summary>
    /// <remarks>
    /// This command embed a set of <see cref="ChangeInfo"/> that must be applied atomically.
    /// </remarks>
    /// <typeparam name="TResourceId">type of the identifier of the resource to patch</typeparam>
    public interface IPatchCommand<TResourceId> : IPatchCommand<Guid, TResourceId, IPatchInfo<TResourceId>>
    {

    }

}