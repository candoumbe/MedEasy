using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.Objects
{

    public class DocumentMetadata : AuditableEntity<int, DocumentMetadata>
    {
        /// <summary>
        /// Id of the patient the document belongs to
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// Size of the document in bytes
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Title of the document
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Mimetype of the document
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// Id of the <see cref="Document"/>
        /// </summary>
        /// <remarks>
        /// This is the bynary content
        /// </remarks>
        public int DocumentId { get; set; }


        public virtual Patient Patient { get; set; }

        /// <summary>
        /// The binary content the current instance is a metadata
        /// </summary>
        public virtual Document Document { get; set; }
    }
}
