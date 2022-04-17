namespace MedEasy.Wasm.Apis
{
    /// <summary>
    /// Type of operation that can be performed as part of a patch document.
    /// </summary>
    public enum PatchOperationType
    {
        /// <summary>
        /// Indicates that the value at the given path should be replaced with the given value.
        /// </summary>
        Add,
        
        /// <summary>
        /// Indicates that the value at the given path should be removed.
        /// </summary>
        Remove,

        /// <summary>
        /// Indicates that the value at the given path should be updated by applying the given operation.
        /// </summary>
        Replace,

        /// <summary>
        /// Indicates that the value at the given path should be updated by applying the given operation.
        /// </summary>
        Move,

        /// <summary>
        /// Indicates that the value at the given path should be updated by applying the given operation.
        /// </summary>
        Copy,

        /// <summary>
        /// Indicates that the value at the given path should be equal to expected value..
        /// </summary>
        Test
    }
}
