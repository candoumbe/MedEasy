using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.DTO
{
    /// <summary>
    /// DTO that store elements to get a list of <see cref="AccountInfo"/> given tenant identifier
    /// </summary>
    public class GetPageOfAccountInfoByTenantIdInfo
    {
        public int Page { get; set; }

        public int PageSize { get; set; }

        public Guid TenantId { get; set; }
    }
}
