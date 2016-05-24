using System;
using System.Collections.Generic;

namespace MedEasy.Validators.Exceptions
{
    public class ValidationException : Exception
    {
        public IEnumerable<ErrorInfo> Errors { get; }
        public ValidationException(string message, IEnumerable<ErrorInfo> errors) : base(message)
        {
            Errors = errors;
        }
    }
}