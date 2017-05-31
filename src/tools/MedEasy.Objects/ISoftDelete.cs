using System;
using System.Collections.Generic;
using System.Text;

namespace MedEasy.Objects
{
    /// <summary>
    /// Add supports for logical deletion of a entity.
    /// </summary>
    public interface ISoftDelete
    {
        /// <summary>
        /// Get/sets if the element is "soft" delete
        /// </summary>
        bool Deleted { get; set; }
    }
}
