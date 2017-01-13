﻿

using System.Collections.Generic;

namespace MedEasy.DTO
{
    /// <summary>
    /// Defines the content of a <see cref="DocumentMetadataInfo"/> upload.
    /// </summary>
    public class CreateDocumentInfo
    {
        public string Title { get; set; }

        public IEnumerable<byte> File { get; set; }
    }
}