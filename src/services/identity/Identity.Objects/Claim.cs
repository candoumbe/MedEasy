namespace Identity.Objects
{
    /// <summary>
    /// A <see cref="Claim"/> is just an element about a <see cref="Account"/>.
    /// <param>
    /// Several claims, usually of various types can be associated to an account.
    /// </param>
    /// </summary>
    public class Claim
    {
        /// <summary>
        /// Type of claim
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Value of the claim
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Builds a new <see cref="Claim"/> with the specified <paramref name="type"/> and <paramref name="value"/>.
        /// </summary>
        /// <param name="type">Type of the claim</param>
        /// <param name="value">Value of the claim</param>
        public Claim(string type, string value)
        {
            Type = type;
            Value = value;
        }

        /// <summary>
        /// Change the <see cref="Value"/> of the current instance.
        /// </summary>
        /// <param name="newValue">The new value to give to the current instance.</param>
        public void ChangeValueTo(string newValue) => Value = newValue;
    }
}