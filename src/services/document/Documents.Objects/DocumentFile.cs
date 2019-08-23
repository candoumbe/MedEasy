using System;

namespace Documents.Objects
{
    public class DocumentFile
    {
        public byte[] Content { get; }

        /// <summary>
        /// Buids a new <see cref="DocumentFile"/>
        /// </summary>
        /// <param name="documentId">id of the <see cref="Document"/> which this content is attached to</param>
        /// <param name="content">Binary content</param>
        public DocumentFile(byte[] content)
        {
            if (content == default)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (content.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(content), $"{nameof(content)} cannot be empty");
            }
            
            Content = content;
        }

        public static implicit operator DocumentFile(byte[] content) => new DocumentFile(content);
    }
}
