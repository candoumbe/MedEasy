using Microsoft.AspNetCore.JsonPatch;

namespace MedEasy.DTO
{
    /// <summary>
    /// Represents a set of changes to apply atomically.
    /// </summary>
    /// <typeparam name="TResourceId">Type of the identifier of the resource to update</typeparam>
    public interface IPatchInfo<TResourceId, TResource> where TResource : class
    {
        /// <summary>
        /// Id of the resource
        /// </summary>
        TResourceId Id { get; }

        /// <summary>
        /// Patch document to apply to the resource
        /// </summary>
        JsonPatchDocument<TResource> PatchDocument { get; }
    }
}
