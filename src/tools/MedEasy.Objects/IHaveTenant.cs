using System;

namespace MedEasy.Objects
{
    /// <summary>
    /// Add multenancy support for an entity.
    /// </summary>
    public interface IHaveTenant
    {
        /// <summary>
        /// Id of the owner of the resource
        /// </summary>
        Guid TenantId { get; set; }
    }
}