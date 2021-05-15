namespace Agenda.Objects.Exceptions
{
    using System;

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
