namespace Identity.Objects
{
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

        public Claim(string type, string value)
        {
            Type = type;
            Value = value;
        }

        public void ChangeValueTo(string newValue) => Value = newValue;
    }
}