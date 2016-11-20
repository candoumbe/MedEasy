using System;
using MedEasy.DTO;

namespace MedEasy.Commands
{
    /// <summary>
    /// Command to partially update a resource
    /// </summary>
    /// <remarks>
    /// This command embed a set of <see cref="PatchOperation"/> that must be applied atomically.
    /// </remarks>
    /// <typeparam name="TKey">type of the command key</typeparam>
    /// <typeparam name="TResourceId">type of the identifier of resource to patch</typeparam>
    /// <typeparam name="TData">type of the data the command carries</typeparam>
    public interface IPatchCommand<TKey, TResourceId, TResource, TData> : ICommand<TKey, TData>
        where TKey : IEquatable<TKey>
        where TData : IPatchInfo<TResourceId, TResource>
        where TResource : class
    {
    }


    /// <summary>
    /// Command to partially update a resource.
    /// </summary>
    /// <remarks>
    /// This command embed a set of <see cref="PatchOperation"/> that must be applied atomically.
    /// </remarks>
    /// <typeparam name="TResourceId">type of the identifier of the resource to patch</typeparam>
    /// <typeparam name="TData">type of the data the command carries</typeparam>
    public interface IPatchCommand<TResourceId, TResource, TData> : IPatchCommand<Guid, TResourceId, TResource, TData>
        where TData : IPatchInfo<TResourceId, TResource>
        where TResource : class
    {

    }


    /// <summary>
    /// Command to partially update a resource.
    /// </summary>
    /// <remarks>
    /// This command embed a set of <see cref="PatchOperation"/> that must be applied atomically.
    /// </remarks>
    /// <typeparam name="TResourceId">type of the identifier of the resource to patch</typeparam>
    /// <typeparam name="TResource">type of the resource to PATCH</typeparam>
    public interface IPatchCommand<TResourceId, TResource> : IPatchCommand<Guid, TResourceId, TResource, IPatchInfo<TResourceId, TResource>>
        where TResource : class
    {

    }

}