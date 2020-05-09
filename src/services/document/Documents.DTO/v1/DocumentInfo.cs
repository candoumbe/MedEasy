using MedEasy.RestObjects;
using System;

namespace Documents.DTO.v1
{
    public class DocumentInfo : Resource<Guid>
    {
        /// <summary>
        /// Name of the document
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Mime type of the document
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// SHA256 hash of the document
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Size of the document (in bytes)
        /// </summary>
        public long Size { get; set; }
    }
}
