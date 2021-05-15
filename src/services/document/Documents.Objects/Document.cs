namespace Documents.Objects
{
    using Documents.Ids;

    using MedEasy.Objects;

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A document submitted in the system
    /// </summary>
    public class Document : AuditableEntity<DocumentId, Document>
    {
        /// <summary>
        /// Name of the document
        /// </summary>
        public string Name { get; private set; }

        private readonly IList<DocumentPart> _parts;

        /// <summary>
        /// Document's parts
        /// </summary>
        public IEnumerable<DocumentPart> Parts => _parts;

        /// <summary>
        /// Gets the MOME type of the document
        /// </summary>
        public string MimeType { get; private set; }

        /// <summary>
        /// SHA256 hash of the document
        /// </summary>
        public string Hash { get; private set; }

        /// <summary>
        /// Size of the document (in bytes)
        /// </summary>
        public long Size { get; private set; }

        /// <summary>
        /// <see cref="Objects.Status"/> of the document
        /// </summary>
        public Status Status { get; private set; }

        /// <summary>
        /// Default MIME type
        /// </summary>
        public const string DefaultMimeType = "application/octect-stream";

        /// <summary>
        /// Builds a new <see cref="Document"/> which <see cref="Status"/> is <see cref="Status.Ongoing"/>.
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="name"></param>
        /// <param name="mimeType"></param>
        public Document(DocumentId id, string name, string mimeType = DefaultMimeType) : base(id)
        {
            Name = name;
            MimeType = mimeType;
            Status = Status.Ongoing;
            _parts = new List<DocumentPart>();
        }

        /// <summary>
        /// Updates the <see cref="Name"/> of the document
        /// </summary>
        /// <param name="newName">new name of the document</param>
        /// <exception cref="ArgumentNullException"><paramref name="newName"/> is null/empty/whitespace only.</exception>
        public void ChangeNameTo(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentNullException(nameof(newName));
            }

            Name = newName;
        }

        /// <summary>
        /// Checks if the current instance holds same <see cref="File"/> as <see cref="other"/>
        /// </summary>
        /// <param name="other"></param>
        /// <returns><c>true</c> if <see cref="other"/> has same content as the current instance 
        /// and <c>false</c> otherwise</returns>
        public bool IsEquivalentTo(Document other) => Hash.Equals(other.Hash);

        /// <summary>
        /// Updates the <see cref="MimeType"/> of the document
        /// </summary>
        /// <param name="mimeType"></param>
        public void ChangeMimeTypeTo(string mimeType) => MimeType = mimeType;

        /// <summary>
        /// Update the size of the document
        /// </summary>
        /// <param name="newSize">The new size of the document</param>
        /// <exception cref="InvalidOperationException">if <see cref="Status"/> is <see cref="Status.Done"/>.</exception>   
        public void UpdateSize(long newSize)
        {
            if (Status == Status.Done)
            {
                throw new InvalidOperationException("The size of the document cannot be changed when its status is Done");
            }

            if (newSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newSize), newSize, $"{nameof(Size)} cannot be negative.");
            }

            Size = newSize;
        }

        /// <summary>
        /// Updates the <see cref="Hash"/> of the document.
        /// </summary>
        /// <param name="newHash">The new hash</param>
        /// <exception cref="InvalidOperationException">if <see cref="Status"/> is <see cref="Status.Done"/></exception>
        public void UpdateHash(string newHash)
        {
            if (Status == Status.Done)
            {
                throw new InvalidOperationException("Cannot change the hash of the document because it's already locked");
            }

            if (newHash is null)
            {
                throw new ArgumentNullException(nameof(newHash));
            }
            Hash = newHash;
        }

        /// <summary>
        /// Changes <see cref="Document"/>' <see cref="Status"/> to <see cref="Status.Done"/>.
        /// <para>
        /// After calling this method, any call to <see cref="UpdateHash(string)"/>, <see cref="UpdateSize(long)"/> will throw <see cref="InvalidOperationException"/>.
        /// </para>
        /// </summary>
        public void Lock() => Status = Status.Done;
    }
}
