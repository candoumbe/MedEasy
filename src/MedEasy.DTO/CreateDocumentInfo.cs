

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedEasy.DTO
{
    /// <summary>
    /// Defines the content of a <see cref="DocumentMetadataInfo"/> upload.
    /// </summary>
    public class CreateDocumentInfo
    {
        [Required]
        [MaxLength(256)]
        public string Title { get; set; }

        [Required]
        [MaxLength(256)]
        public string MimeType { get; set; }

        /// <summary>
        /// The binary content of the document
        /// </summary>
        public byte[] Content { get; set; }
    }
}