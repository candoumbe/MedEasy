namespace MedEasy.Wasm.Apis
{
    /// <summary>
    /// A model to submit when updating a resource using a <c>PATCH</c>.
    /// </summary>
    /// <typeparam name="TKey">Type of the identifier of the resource to patch</typeparam>
    public class PatchDocumentModel<TKey>
    {
        /// <summary>
        /// Gets or sets the identifier of the resource to patch
        /// </summary>
        public TKey Id { get; init; }

        /// <summary>
        /// Set of operations to be performed on a resource identified by <see cref="Id"/>.
        /// </summary>
        public IEnumerable<PatchOperation> Operations { get; init; }
    }
}
