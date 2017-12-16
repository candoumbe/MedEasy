using MedEasy.DTO;
using System;

namespace MedEasy.CQRS.Core.Commands
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
    public interface IPatchCommand<TCommandId, TResourceId, TResource> : ICommand<TCommandId, PatchInfo<TResourceId, TResource>, Nothing>
        where TCommandId : IEquatable<TCommandId>
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
    public interface IPatchCommand<TResourceId, TResource> : IPatchCommand<Guid, TResourceId, TResource>
        where TResource : class
    {

    }
}
