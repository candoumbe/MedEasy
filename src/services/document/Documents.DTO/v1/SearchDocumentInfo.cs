using MedEasy.DTO.Search;
using System;
using System.Collections.Generic;
using System.Text;

namespace Documents.DTO.v1
{
    public class SearchDocumentInfo : AbstractSearchInfo<DocumentInfo>
    {
        public string Name { get; set; }

        public string MimeType { get; set; }
    }
}
