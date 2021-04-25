using Identity.Ids;

using MedEasy.Objects;

namespace Identity.Objects
{
    /// <summary>
    /// Association between a <see cref="Claim"/> and a <see cref="Role"/>.
    /// </summary>
    public class RoleClaim : Entity<RoleClaimId, RoleClaim>
    {
        public Claim Claim { get; private set; }

        public RoleId RoleId { get; }

        private RoleClaim(RoleId roleId, RoleClaimId id) : base(id)
        {
            RoleId = roleId;
        }

        /// <summary>
        /// Builds a new <see cref="RoleClaim"/> instance.
        /// </summary>
        /// <param name="roleId">id of the role the <see cref="Claim"/> is attached to</param>
        /// <param name="id">id of the claim</param>
        /// <param name="type">type of the claim</param>
        /// <param name="value">value of the claim</param>
        public RoleClaim(RoleId roleId, RoleClaimId id, string type, string value) : this(roleId, id)
        {
            Claim = new Claim(type, value);
        }
    }
}