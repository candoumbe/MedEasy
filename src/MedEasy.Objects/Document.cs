using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.Objects
{
    /// <summary>
    /// The binary content associated to a <see cref="DocumentMetadata"/>
    /// </summary>
    public class Document: AuditableBaseEntity<Document>
    {

        /// <summary>
        /// ID of the related <see cref="DocumentMetadata"/>
        /// </summary>
        /// <remarks>
        /// </remarks>
        public int DocumentMetadataId { get; set; }

        /// <summary>
        /// Binary content
        /// </summary>
        public byte[] Content { get; set; }

        /// <summary>
        /// The document metadata.
        /// </summary>
        public DocumentMetadata DocumentMetadata { get; set; }

        /// <summary>
        /// Unique identifier of the <see cref="Document"/>.
        /// </summary>
        public Guid UUID { get; set; }



    }
}
