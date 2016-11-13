

using System.Collections.Generic;

namespace MedEasy.DTO
{
    /// <summary>
    /// Embeds a set of change to apply to a resource
    /// </summary>
    /// <typeparam name="TResourceId"></typeparam>
    public class PatchInfo<TResourceId> : IPatchInfo<TResourceId>
    {
        /// <summary>
        /// Id of the resource to apply change on
        /// </summary>
        public TResourceId Id { get; set; }

        /// <summary>
        /// Set of changes to apply
        /// </summary>
        public IEnumerable<ChangeInfo> Changes { get; set; }

        /// <summary>
        /// Builds a new <see cref="PatchInfo{TResourceId}"/> instance.
        /// </summary>
        public PatchInfo()
        {
            Changes = new List<ChangeInfo>();
        }
    }
}