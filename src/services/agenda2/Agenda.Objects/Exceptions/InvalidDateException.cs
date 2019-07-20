using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agenda.Objects.Exceptions
{
    /// <summary>
    /// Exception thrown when passing invalid start/end date when creating/modifying an appointment
    /// </summary>
    public class InvalidDateException : Exception
    {
        public InvalidDateException(string message) : base(message)
        {

        }
    }
}
