using System.Collections.Generic;

namespace MedEasy.RestObjects
{
    /// <summary>
    /// Defines the generic shape of a response that holds a portion of more large result set
    /// </summary>
    /// <typeparam name="T">Type of items</typeparam>
    public interface IGenericPagedGetResponse<T> : IGetResponse<T>
    {
        /// <summary>
        /// Number of items in the result set that the current <see cref="Items"/> is just a portion.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Items of the result
        /// </summary>
        IEnumerable<T> Items { get; }

        /// <summary>
        /// Link to navigate through the result set
        /// </summary>
        PageLinks Links { get; }
    }
}