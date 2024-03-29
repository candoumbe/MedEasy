﻿namespace MedEasy.CQRS.Core.Exceptions
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents an error
    /// </summary>
    [JsonObject]
    public class ErrorInfo
    {
        /// <summary>
        /// Key
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// Description of the error
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Severity of the error
        /// </summary>
        public ErrorLevel Severity { get; set; }

        /// <summary>
        /// Builds a new <see cref="ErrorInfo"/> instance.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="description"></param>
        /// <param name="severity"></param>
        public ErrorInfo(string key, string description, ErrorLevel severity)
        {
            Key = key;
            Description = description;
            Severity = severity;
        }
    }
}
