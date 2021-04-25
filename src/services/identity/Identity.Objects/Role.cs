using Identity.Ids;

using MedEasy.Objects;

using Optional;
using Optional.Collections;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Identity.Objects
{
    public class Role : AuditableEntity<RoleId, Role>
    {
        public string Code { get; set; }

        /// <summary>
        /// Claims associated with the current <see cref="Role"/>
        /// </summary>
        public IEnumerable<RoleClaim> Claims => _claims;

        private readonly IList<RoleClaim> _claims;

        /// <summary>
        /// <see cref="Account"/>s associated with the current role
        /// </summary>
        public IEnumerable<AccountRole> Accounts { get; }

        /// <summary>
        /// Builds a new <see cref="Role"/> instance.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="code"></param>
        public Role(RoleId id, string code) : base(id ?? throw new ArgumentNullException(nameof(id)))
        {
            if (id == RoleId.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }
            Code = code ?? throw new ArgumentNullException(nameof(code));
            _claims = new List<RoleClaim>();
        }

        /// <summary>
        /// Adds or update a claim associated to the current role
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public void AddOrUpdateClaim(string type, string value)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Option<RoleClaim> optionRoleClaim = _claims.SingleOrNone(rc => rc.Claim.Type == type);

            optionRoleClaim.Match(
                some: rc => rc.Claim.ChangeValueTo(value),
                none: () => _claims.Add(new RoleClaim(Id, RoleClaimId.New(), type, value))
            );
        }

        /// <summary>
        /// Removes all the claims with the specified type.
        /// </summary>
        /// <param name="type">type of claim to remove</param>
        public void RemoveClaim(string type)
        {
            IEnumerable<RoleClaim> claimsToRemove = _claims.Where(rc => rc.Claim.Type == type)
                                                           .ToArray();

            foreach (RoleClaim rc in claimsToRemove)
            {
                _claims.Remove(rc);
            }
        }

        /// <summary>
        /// Removes only the claim with the specified <paramref name="type"/> and <paramref name="value"/>.
        /// </summary>
        /// <param name="type">type of claim to remove</param>
        /// <param name="value">value of claim to remove</param>
        public void RemoveClaim(string type, string value)
        {
            IEnumerable<RoleClaim> claimsToRemove = _claims.Where(rc => rc.Claim.Type == type && rc.Claim.Value == value)
                                                           .ToArray();

            foreach (RoleClaim rc in claimsToRemove)
            {
                _claims.Remove(rc);
            }
        }
    }
}