using System.Collections.Generic;

namespace MedEasy.DTO
{
    /// <summary>
    /// Represents a set of changes to apply atomically.
    /// </summary>
    /// <typeparam name="TResourceId">Type of the identifier of the resource to update</typeparam>
    public interface IPatchInfo<TResourceId>
    {
        /// <summary>
        /// Id of the resource
        /// </summary>
        TResourceId Id { get; }

        /// <summary>
        /// Set of <see cref="ChangeInfo"/> to apply.
        /// </summary>
        IEnumerable<ChangeInfo> Changes { get; }
    }
}
