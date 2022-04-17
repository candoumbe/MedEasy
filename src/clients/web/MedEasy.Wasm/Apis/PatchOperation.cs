namespace MedEasy.Wasm.Apis
{
    /// <summary>
    /// A single patch operation to be performed on a resource.
    /// </summary>
    public record PatchOperation
    {
        /// <summary>
        /// Builds a new <see cref="PatchOperation"/> instance.
        /// </summary>
        /// <param name="op">The type of operation</param>
        /// <param name="path">Name of the property</param>
        /// <param name="value">The value of the operation</param>
        public PatchOperation(PatchOperationType op, string path, object value)
        {
            Op = op;
            Path = $"/{path}";
            Value = value;
        }
        
        /// <summary>
        /// Indicates the operation to perform.
        /// </summary>
        public PatchOperationType Op { get; init; }

        /// <summary>
        /// Indicates the path to the property to update.
        /// </summary>
        public string Path { get; init; }

        /// <summary>
        /// The value to set the property to.
        /// </summary>
        public object Value { get; init; }
    }
}
