using System;
using MedEasy.DTO;
using MedEasy.CQRS.Core;

namespace MedEasy.Commands
{
    /// <summary>
    /// Command to partially update a resource
    /// </summary>
    /// <remarks>
    /// This command embed a set of <see cref="PatchOperation"/> that must be applied atomically.
    /// </remarks>
    /// <typeparam name="TCommandId">type of the command key</typeparam>
    /// <typeparam name="TResourceId">type of the identifier of resource to patch</typeparam>
    /// <typeparam name="TResource">type of the resource resource to patch</typeparam>
    /// <typeparam name="TCommandData">type of the data the command carries</typeparam>
    public interface IPatchCommand<TCommandId, TResourceId, TResource, TCommandData> : ICommand<TCommandId, TCommandData, Nothing>
        where TCommandId : IEquatable<TCommandId>
        where TCommandData : IPatchInfo<TResourceId, TResource>
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