namespace Documents.DTO.v1
{
    using MedEasy.DTO.Search;

    public class SearchDocumentInfo : AbstractSearchInfo<DocumentInfo>
    {
        public string Name { get; set; }

        public string MimeType { get; set; }
    }
}
