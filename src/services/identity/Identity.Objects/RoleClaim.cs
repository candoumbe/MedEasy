using MedEasy.Objects;
using System;

namespace Identity.Objects
{
    /// <summary>
    /// Association between a <see cref="Claim"/> and a <see cref="Role"/>
    /// </summary>
    public class RoleClaim : AuditableEntity<int, RoleClaim>
    {
        public int RoleId { get; set; }

        public int ClaimId { get; set; }

        /// <summary>
        /// Role of the association
        /// </summary>
        public Role Role { get; private set; }

        /// <summary>
        /// <see cref="Claim"/> of the associtation
        /// </summary>
        public Claim Claim { get; set; }

        public RoleClaim(Guid uuid, Role role, Claim claim) : base(uuid)
        {
            Role = role;
            Claim = claim;
            ClaimId = claim.Id;
            RoleId = role.Id;
        }

        public RoleClaim(Guid uuid, int roleId, int claimId) : base(uuid)
        {
            ClaimId = claimId;
            RoleId = roleId;
        }


    }
}