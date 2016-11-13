using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.DTO
{
    /// <summary>
    /// A type of change to apply to a property.
    /// </summary>
    public enum ChangeInfoType
    {
        /// <summary>
        /// Operation to add something to a collection
        /// </summary>
        Add,
        
        /// <summary>
        /// Operation to remove something from a collection
        /// </summary>
        Remove,

        /// <summary>
        /// Operation to change something either in a collection or in a single property from one value to an other
        /// </summary>
        Update,
    }
}
