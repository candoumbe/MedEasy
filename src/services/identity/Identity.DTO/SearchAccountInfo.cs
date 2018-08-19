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
        /// Filter to apply on <see cref="AccountInfo.Username"/> property
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Filter to apply on <see cref="AccountInfo.Email"/> property
        /// </summary>
        public string Email { get; set; }


        /// <summary>
        /// Filter to apply on <see cref="AccountInfo.Name"/> property
        /// </summary>
        public string Name { get; set; }
    }
}
