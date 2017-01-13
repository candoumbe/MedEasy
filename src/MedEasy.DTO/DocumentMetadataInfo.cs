namespace MedEasy.DTO
{
    /// <summary>
    /// Metadata of a document
    /// </summary>
    public class DocumentMetadataInfo : ResourceBase<int>
    {
        /// <summary>
        /// Size of the file
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Id of the patient the document belongs to
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// Title of the document
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Mimetype of the document
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// Id of the binary content
        /// </summary>
        public int DocumentId { get; set; }

    }
}
