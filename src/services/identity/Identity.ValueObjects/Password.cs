namespace Identity.ValueObjects
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// A value object that wraps a password
    /// </summary>
    public sealed class Password
    {
        /// <summary>
        /// Value of the password
        /// </summary>
        public string Value { get; }

        private Password(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new <see cref="Password"/> instance.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">when <paramref name="input"/> is <c>null</c> or whitespace.</exception>
        public static Password From(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentNullException(nameof(input));
            }

            return new (input);
        }

        ///<inheritdoc/>
        public override string ToString() => "******";
    }
}
