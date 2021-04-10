using MedEasy.DTO.Search;

namespace Documents.DTO.v1
{
    public class SearchDocumentInfo : AbstractSearchInfo<DocumentInfo>
    {
        public string Name { get; set; }

        public string MimeType { get; set; }
    }
}
