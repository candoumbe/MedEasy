using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.Validators
{
    /// <summary>
    /// Static clas
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Validator<T> : IValidate<T>
    {
        public IEnumerable<Task<ErrorInfo>> Validate(T element) => Enumerable.Empty<Task<ErrorInfo>>();

        private Validator()
        {}

        private static readonly Lazy<Validator<T>> lazy = new Lazy<Validator<T>>(() => new Validator<T>());

        /// <summary>
        /// Gets the default validator instance for the specified type
        /// </summary>
        public static Validator<T> Default => lazy.Value;
    }
}
