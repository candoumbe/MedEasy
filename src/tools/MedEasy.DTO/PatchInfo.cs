using Microsoft.AspNetCore.JsonPatch;

namespace MedEasy.DTO
{
    /// <summary>
    /// Embeds a set of change to apply to a resource
    /// </summary>
    /// <typeparam name="TResourceId"></typeparam>
    public class PatchInfo<TResourceId, TResource> where TResource : class
    {
        /// <summary>
        /// Id of the resource to apply change on
        /// </summary>
        public TResourceId Id { get; set; }

        /// <summary>
        /// Set of changes to apply
        /// </summary>
        public JsonPatchDocument<TResource> PatchDocument { get; set; }

    }
}