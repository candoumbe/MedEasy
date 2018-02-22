using System.Collections.Generic;

namespace MedEasy.RestObjects
{
    /// <summary>
    /// This interface describes the properties a browsable resource must implements
    /// </summary>
    public interface IBrowsableResource<T>
    {
       /// <summary>
       /// Links associated with the resource
       /// </summary>
       /// <remarks>
       /// This property can be used by <see cref="EnvelopeFilterAttribute"/> to set HTTP response HEADERS
       /// </remarks>
        IEnumerable<Link> Links{ get; }

        /// <summary>
        /// The resource
        /// </summary>
        T Resource { get; }

    }
}
