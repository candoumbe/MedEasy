using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Measures.Objects.Exceptions
{
    /// <summary>
    /// Exception thrown when adding a item in a collection but the id of that item already exists.
    /// </summary>
    public class DuplicateIdException : Exception
    {
        
    }
}
