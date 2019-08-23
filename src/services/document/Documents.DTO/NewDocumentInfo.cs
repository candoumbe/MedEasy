using MedEasy.RestObjects;
using System;

namespace Documents.DTO
{
    public class NewDocumentInfo : Resource<Guid>, IEquatable<NewDocumentInfo>
    {
        public string Name { get; set; }

        public string MimeType { get; set; }

        public byte[] Content { get; set; }

        public bool Equals(NewDocumentInfo other) => (MimeType, Content) == (other?.MimeType, other?.Content);

        public override bool Equals(object obj) => Equals(obj as NewDocumentInfo);

        public override int GetHashCode() => (MimeType, Content).GetHashCode()  ;
    }
}
