using MedEasy.RestObjects;

namespace MedEasy.DTO
{
    /// <summary>
    /// This interface describes the properties a browsable resource must implements
    /// </summary>
    public interface IBrowsableResource<T>
    {
       /// <summary>
       /// Location of the resource
       /// </summary>
        Link Location { get; }

        /// <summary>
        /// The resource
        /// </summary>
        T Resource { get; }

    }
}
