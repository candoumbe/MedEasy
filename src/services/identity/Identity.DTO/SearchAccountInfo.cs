using MedEasy.DTO.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.DTO
{
    public class SearchAccountInfo : AbstractSearchInfo<AccountInfo>
    {
        /// <summary>
        /// Filter to apply on username
        /// </summary>
        public string UserName { get; set; }
    }
}
