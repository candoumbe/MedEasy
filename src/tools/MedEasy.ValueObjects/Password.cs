namespace MedEasy.ValueObjects
{
    using System;
    using System.ComponentModel;
#if NET5_0_OR_GREATER
    using System.Text.Json.Serialization;

    using MedEasy.ValueObjects.Converters.SystemTextJson;
#endif
    /// <summary>
    /// A value object that wraps a password
    /// </summary>
#if NET5_0_OR_GREATER
    [JsonConverter(typeof(PasswordJsonConverter))]
#endif
    [TypeConverter(typeof(PasswordTypeConverter))]
    public record Password
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

            return new(input);
        }

        ///<inheritdoc/>
        public override string ToString() => "******";
    }
}
