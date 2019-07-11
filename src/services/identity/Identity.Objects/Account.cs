using MedEasy.Objects;
using Optional;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

namespace Identity.Objects
{
    public class Account : AuditableEntity<Guid, Account>, IMayHaveTenant

    {
        /// <summary>
        /// Login used to authenticate
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Salt used when computing <see cref="PasswordHash"/>
        /// </summary>
        public string Salt { get; private set; }

        /// <summary>
        /// Hash of the user's password
        /// </summary>
        public string PasswordHash { get; private set; }

        /// <summary>
        /// Email associated with the account
        /// </summary>
        public string Email { get; private set; }

        public string Name { get; set; }

        public bool EmailConfirmed { get; private set; }

        public string RefreshToken { get; private set; }

        /// <summary>
        /// Indicates if the account can be used to authenticate a user
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Indicates if <see cref="Account"/> is locked out.
        /// </summary>
        /// <remarks>
        /// A locked <see cref="Account"/> cannot log in.
        /// </remarks>
        public bool Locked { get; private set; }

        /// <summary>
        /// Id of the owner of the element
        /// </summary>
        public Guid? TenantId { get; private set; }

        private readonly IDictionary<string, Role> _roles;

        private readonly IDictionary<string, (string value, DateTimeOffset start, DateTimeOffset? end)> _claims;

        public IEnumerable<Role> Roles => _roles.Values;

        public IReadOnlyDictionary<string, (string value, DateTimeOffset start, DateTimeOffset? end)> Claims => new ReadOnlyDictionary<string, (string value, DateTimeOffset start, DateTimeOffset? end)>(_claims);

        /// <summary>
        /// Builds a new <see cref="Account"/> instance
        /// </summary>
        public Account(Guid id, string username, string email, string passwordHash, string salt, string name="", bool locked = false, bool isActive = false, Guid? tenantId = null, string refreshToken = null) : base(id)
        {
            Username = username;
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
            Salt = salt;
            Locked = locked;
            IsActive = isActive;
            TenantId = tenantId;
            RefreshToken = refreshToken;
            _roles = new ConcurrentDictionary<string, Role>();
            _claims = new ConcurrentDictionary<string, (string value, DateTimeOffset start, DateTimeOffset? end)>();
        }

        /// <summary>
        /// Set password
        /// </summary>
        /// <param name="passwordHash"></param>
        /// <param name="salt"></param>
        public void SetPassword(string passwordHash, string salt)
        {
            PasswordHash = passwordHash;
            Salt = salt;
        }

        public void ChangeRefreshToken(string refreshToken)
        {
            RefreshToken = refreshToken;
        }

        /// <summary>
        /// Removes the <see cref="RefreshToken"/> previously associated with the current <see cref="Account"/>.
        /// </summary>
        public void DeleteRefreshToken() => RefreshToken = null;

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
            
            if (end != default && start > end)
            {
                throw new ArgumentOutOfRangeException(nameof(start), "start > end");
            }

            if (_claims.ContainsKey(type))
            {
                (string value, DateTimeOffset start, DateTimeOffset? end) claim = _claims[type];
                _claims.Remove(type);
                _claims.Add(type, (value, claim.start, claim.end));
            }
            else
            {
                _claims.Add(type, (value, start, end));
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

        /// <summary>
        /// Defines the tenant of the current element
        /// </summary>
        /// <param name="tenantId"></param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="tenantId"/> is <see cref="Guid.Empty"/></exception>
        public void SetTenant(Guid? tenantId)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(tenantId));
            }
            TenantId = tenantId;
        }
    }
}
