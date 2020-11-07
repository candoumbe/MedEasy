using System;

namespace MedEasy.Objects
{
    /// <summary>
    /// Adds multitenancy support
    /// </summary>
    public interface IMayHaveTenant
    {
        Guid? TenantId { get; }

        /// <summary>
        /// Defines the <see cref="Tenantid"/>
        /// </summary>
        /// <param name="tenantId"></param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="tenantId"/> is <see cref="Guid.Empty"/></exception>
        void OwnsBy(Guid? tenantId);
    }
}
