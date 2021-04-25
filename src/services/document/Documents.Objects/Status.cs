namespace Documents.Objects
{
    /// <summary>
    /// Status of the documents in the Storage
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// <see cref="Status"/> of <see cref="Document"/>s which upload is ongoing
        /// </summary>
        Ongoing,

        /// <summary>
        /// <see cref="Status"/> of <see cref="Document"/>s which upload is finished.
        /// </summary>
        Done
    }
}
