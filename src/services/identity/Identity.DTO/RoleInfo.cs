using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.DTO
{
    public class RoleInfo
    {
        /// <summary>
        /// Name of the role
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Claims associated with the role
        /// </summary>
        public IEnumerable<ClaimInfo> Claims { get; set; }
    }
}
