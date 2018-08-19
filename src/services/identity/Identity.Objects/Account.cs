using MedEasy.Objects;
using Optional;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Identity.Objects
{
    public class Account : AuditableEntity<int, Account>, IMayHaveTenant

    {
        /// <summary>
        /// Login used to authenticate
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Salt used when computing <see cref="PasswordHash"/>
        /// </summary>
        public string Salt { get; set; }


        /// <summary>
        /// Hash of the user's password
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Email associated with the account
        /// </summary>
        public string Email { get; set; }

        public string Name { get; set; }

        public bool EmailConfirmed { get; set; }


        public string RefreshToken { get; set; }

        /// <summary>
        /// Indicates if the account can be used to authenticate a user
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Indicates if <see cref="Account"/> is locked out.
        /// </summary>
        /// <remarks>
        /// A locked <see cref="Account"/> cannot log in.
        /// </remarks>
        public bool Locked { get; set; }

        /// <summary>
        /// Id of the owner of the element
        /// </summary>
        public Guid? TenantId { get; set; }


        private readonly IDictionary<string, Role> _roles;

        private readonly IDictionary<string, AccountClaim> _claims;

        public IEnumerable<Role> Roles => _roles.Values;

        public IEnumerable<AccountClaim> Claims => _claims.Values;

        /// <summary>
        /// Builds a new <see cref="Account"/> instance
        /// </summary>
        public Account()
        {
            _roles = new ConcurrentDictionary<string, Role>();
            _claims = new ConcurrentDictionary<string, AccountClaim>();
        }

        /// <summary>
        /// Adds a claim to the current <see cref="Account"/> instance.
        /// </summary>
        /// <param name="type">Type of the claim.</param>
        /// <param name="value">Value of the claim</param>
        /// <param name="start">When the claim starts</param>
        /// <param name="end">When the claim ends</param>
        /// <exception cref="ArgumentNullException">if <paramref name="type"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="end"/> is not <c>null</c> 
        /// and <paramref name="start"/> &gt; <paramref name="end"/>
        /// </exception>
        public void AddOrUpdateClaim(string type, string value, DateTimeOffset start, DateTimeOffset? end = null)
        {
            if (type == default)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (end != default && start > end)
            {
                throw new ArgumentOutOfRangeException(nameof(start), "start > end");
            }

            if (_claims.ContainsKey(type))
            {
                _claims[type].Value = value;
            }
            else
            {
                _claims.Add(type, new AccountClaim { Start = start, End = end, Value = value, Claim = new Claim { Type = type, Value = value } });
            }
        }


        /// <summary>
        /// Remove the <see cref="UserClaim"/> with the specified <see cref="UserClaim.Claim.Type"/>.
        /// </summary>
        /// <param name="type">Type of claim to remove</param>
        /// <exception cref="ArgumentNullException">if <paramref name="type"/> is <c>null</c>.</exception>
        public void RemoveClaim(string type)
        {
            if (type == default)
            {
                throw new ArgumentNullException(nameof(type));
            }

            _claims.Remove(type);
            
        }
    }
}
