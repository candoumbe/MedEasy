namespace Identity.DTO
{
    using MedEasy.Ids;

    /// <summary>
    /// DTO that store elements to get a list of <see cref="AccountInfo"/> given tenant identifier
    /// </summary>
    public class GetPageOfAccountInfoByTenantIdInfo
    {
        public int Page { get; set; }

        public int PageSize { get; set; }

        public TenantId TenantId { get; set; }
    }
}
