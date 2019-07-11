using MedEasy.Objects;
using System;
using System.Collections.Generic;

namespace Identity.Objects
{
    public class Role : AuditableEntity<Guid, Role>
    {
        public string Code { get; set; }

        /// <summary>
        /// Claims associated with the current <see cref="Role"/>
        /// </summary>
        public IEnumerable<RoleClaim> Claims { get; set; }

        /// <summary>
        /// <see cref="Account"/>s associated with the current role
        /// </summary>
        public IEnumerable<Account> Users { get; set; }

        /// <summary>
        /// Builds a new <see cref="Role"/> instance.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="code"></param>
        public Role(Guid id, string code): base(id)
        {
            Code = code;
        }
    }
}