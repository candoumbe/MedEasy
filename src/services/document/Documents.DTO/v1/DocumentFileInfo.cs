using MedEasy.RestObjects;
using System;

namespace Documents.DTO.v1
{
    public class DocumentFileInfo : DocumentInfo
    {
        public byte[] Content { get; set; }

    }
}
