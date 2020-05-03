using MedEasy.Objects;
using System;

namespace Identity.Objects
{
    /// <summary>
    /// Association between a <see cref="Claim"/> and a <see cref="Role"/>
    /// </summary>
    public class RoleClaim : Entity<Guid, RoleClaim>
    {
        public Claim Claim { get; private set; }

        public Guid RoleId { get; }

        private RoleClaim(Guid roleId, Guid id) : base(id)
        {
            RoleId = roleId;
        }

        /// <summary>
        /// Builds a new <see cref="RoleClaim"/> instance.
        /// </summary>
        /// <param name="roleId">id pf the role the <see cref="Claim"/></param>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public RoleClaim(Guid roleId, Guid id, string type, string value) : this(roleId, id)
        {
            Claim = new Claim(type, value);
        }
    }
}