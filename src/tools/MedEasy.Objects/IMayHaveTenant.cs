namespace MedEasy.Objects
{
    using MedEasy.Ids;

    /// <summary>
    /// Adds multitenancy support
    /// </summary>
    public interface IMayHaveTenant
    {
        TenantId TenantId { get; }

        /// <summary>
        /// Defines the <see cref="Tenantid"/>
        /// </summary>
        /// <param name="tenantId"></param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="tenantId"/> is <see cref="TenantId.Empty"/></exception>
        void OwnsBy(TenantId tenantId);
    }
}
