using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.Objects
{
    /// <summary>
    /// The binary content
    /// </summary>
    public class Document: AuditableEntity<int, Document>
    {
        /// <summary>
        /// ID of the related <see cref="DocumentMetadata"/>
        /// </summary>
        public int DocumentMetadataId { get; set; }

        /// <summary>
        /// Binary content
        /// </summary>
        public byte[] Content { get; set; }

        /// <summary>
        /// The document metadata
        /// </summary>
        public DocumentMetadata DocumentMetadata { get; set; }
    }
}
