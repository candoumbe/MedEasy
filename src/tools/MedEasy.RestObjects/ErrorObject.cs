using System.Collections.Generic;

namespace MedEasy.RestObjects
{
    /// <summary>
    /// Wraps any errors that a serice wish to send to a client
    /// </summary>
    public class ErrorObject
    {
        /// <summary>
        /// Error code.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Short description of the message
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Validation errors
        /// </summary>
        public IDictionary<string, IEnumerable<string>> Errors { get; set;}

        public ErrorObject()
        {
            Errors = new Dictionary<string, IEnumerable<string>>();
        }
    }
}
