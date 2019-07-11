using MedEasy.Objects;
using System;

namespace Identity.Objects
{
    /// <summary>
    /// Association between a <see cref="Claim"/> and a <see cref="Role"/>
    /// </summary>
    public class RoleClaim : AuditableEntity<Guid, RoleClaim>
    {
        public Guid RoleId { get; set; }

        public Guid ClaimId { get; set; }

        /// <summary>
        /// Role of the association
        /// </summary>
        public Role Role { get; private set; }

        /// <summary>
        /// <see cref="Claim"/> of the associtation
        /// </summary>
        public Claim Claim { get; set; }

        public RoleClaim(Guid id, Role role, Claim claim) : base(id)
        {
            Role = role;
            Claim = claim;
            ClaimId = claim.Id;
            RoleId = role.Id;
        }

        public RoleClaim(Guid id, Guid roleId, Guid claimId) : base(id)
        {
            ClaimId = claimId;
            RoleId = roleId;
        }


    }
}