using MedEasy.RestObjects;
using System;

namespace Documents.DTO.v1
{
    public class DocumentInfo : Resource<Guid>
    {
        public string Name { get; set; }

        public string MimeType { get; set; }

        public string Hash { get; set; }

    }
}
