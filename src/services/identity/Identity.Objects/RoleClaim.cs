using MedEasy.Objects;

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
        public Role Role { get; set; }
        /// <summary>
        /// <see cref="Claim"/> of the associtation
        /// </summary>
        public Claim Claim { get; set; }


    }
}