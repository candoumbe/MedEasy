namespace MedEasy.Objects
{
    using MedEasy.Ids;

    /// <summary>
    /// Add multenancy support for an entity.
    /// </summary>
    public interface IHaveTenant
    {
        /// <summary>
        /// Id of the owner of the resource
        /// </summary>
        TenantId TenantId { get; }
    }
}