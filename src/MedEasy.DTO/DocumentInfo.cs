namespace MedEasy.DTO
{
    /// <summary>
    /// Binary content
    /// </summary>
    public class DocumentInfo : ResourceBase<int>
    {
        /// <summary>
        /// Binary content
        /// </summary>
        public byte[] Content { get; set; }

    }
}
