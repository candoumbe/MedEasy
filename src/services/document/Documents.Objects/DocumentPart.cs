using Documents.Ids;

using System;

namespace Documents.Objects
{
    public class DocumentPart
    {
        public byte[] Content { get; }

        /// <summary>
        /// Position of the content amongst its siblings
        /// </summary>
        public int Position { get; }

        public DocumentId DocumentId { get; }


        public long Size { get; }

        /// <summary>
        /// Buids a new <see cref="DocumentPart"/>
        /// </summary>
        /// <param name="documentId">id of the <see cref="Document"/> which this content is attached to</param>
        /// <param name="position">O-based index of the position of the current instance amongst all other<see cref="DocumentPart"/>s for a same <see cref="Document"/>.</param>
        /// <param name="content">Binary content</param>
        public DocumentPart(DocumentId documentId, int position, byte[] content)
        {
            if (content == default)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (content.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(content), $"{nameof(content)} cannot be empty");
            }

            if (position < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(position), position, "position must be a 0-based index");
            }

            if (documentId == DocumentId.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(documentId), documentId, $"{nameof(documentId)} cannot be empty");
            }

            DocumentId = documentId;
            Content = content;
            Position = position;
            Size = content.Length;
        }
    }
}
