using MedEasy.Objects;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Documents.Objects
{
    /// <summary>
    /// A document submitted in the system
    /// </summary>
    public class Document : AuditableEntity<Guid, Document>
    {
        public string Name { get; }
        public DocumentFile File { get; private set; }
        public string MimeType { get; private set; }
        public string Hash { get; private set; }

        public long Size { get; private set; }

        public const string DefaultMimeType = "application/octect-stream";


        public Document(Guid id, string name, string mimeType = DefaultMimeType) : base(id)
        {
            Name = name;
            MimeType = mimeType;
        }

        public Document SetFile(DocumentFile file)
        {
            File = file ?? throw new ArgumentNullException(nameof(file));
            Size = file.Content.LongLength;
            Hash = Encoding.Default.GetString(SHA256.Create().ComputeHash(file.Content));

            return this;
        }

        /// <summary>
        /// Checks if the current instance holds same <see cref="File"/> as <see cref="other"/>
        /// </summary>
        /// <param name="other"></param>
        /// <returns><c>true</c> if <see cref="other"/> has same content as the current instance 
        /// and <c>false</c> otherwise</returns>
        public bool IsEquivalentTo(Document other) => Hash.Equals(other.Hash);
        public void ChangeMimeTypeTo(string mimeType) => MimeType = mimeType;
    }
}
