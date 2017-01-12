namespace MedEasy.DTO
{
    /// <summary>
    /// Metadata of a document
    /// </summary>
    public class DocumentInfo : ResourceBase<int>
    {
        /// <summary>
        /// Size of the file
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Id of the patient the document belongs to
        /// </summary>
        public int PatientId { get; set; }
    }
}
