namespace Identity.Objects;

using Identity.Ids;
using Identity.ValueObjects;

using MedEasy.Ids;
using MedEasy.Objects;

using NodaTime;

using Optional;
using Optional.Collections;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

/// <summary>
/// An <see cref="Account"/> can be used to authenticate and access the applicatoin.
/// </summary>
public class Account : AuditableEntity<AccountId, Account>, IMayHaveTenant

{
    /// <summary>
    /// Login used to authenticate
    /// </summary>
    public UserName Username { get; private set; }

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
    public Email Email { get; }

    /// <summary>
    /// Name of the account
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Indicates if the current instance <see cref="Email"/> was confirmed.
    /// </summary>
    public bool EmailConfirmed { get; }

    /// <summary>
    /// Token that can be used to request new access tokens.
    /// </summary>
    public string RefreshToken { get; private set; }

    /// <summary>
    /// Indicates if the account can be used to authenticate a user
    /// </summary>
    public bool IsActive { get; }

    /// <summary>
    /// Indicates if <see cref="Account"/> is locked out.
    /// </summary>
    /// <remarks>
    /// A locked <see cref="Account"/> cannot log in.
    /// </remarks>
    public bool Locked { get; }

    /// <summary>
    /// Id of the owner of the element
    /// </summary>
    public TenantId TenantId { get; private set; }

    private readonly IList<AccountRole> _roles;

    private readonly IList<AccountClaim> _claims;

    /// <summary>
    /// <see cref="AccountRole"/>s associated to the current <see cref="Account"/>.
    /// </summary>
    public IEnumerable<AccountRole> Roles => _roles.ToImmutableArray();

    /// <summary>
    /// <see cref="Claim"/>s associated with the current <see cref="Account"/>
    /// </summary>
    public IEnumerable<AccountClaim> Claims => _claims.ToImmutableArray();

    /// <summary>
    /// Builds a new <see cref="Account"/> instance
    /// </summary>
    /// <param name="id">Account's identifier</param>
    /// <param name="username">Username associated with the account</param>
    /// <param name="email">Email of the account</param>
    /// <param name="passwordHash">Hash of the password associated with the account</param>
    /// <param name="salt">Salt used to further obfuscate the account</param>
    /// <param name="name">Name of the account</param>
    /// <param name="locked">Indicates if the account is locked or not.</param>
    /// <param name="isActive">Indicates if the account is active.</param>
    /// <param name="tenantId">Identifier of the owner of the account.</param>
    /// <param name="refreshToken">Token that can be used to give the account a new access token.</param>
    public Account(AccountId id,
                   UserName username,
                   Email email,
                   string passwordHash,
                   string salt,
                   string name = "",
                   bool locked = false,
                   bool isActive = false,
                   TenantId tenantId = null,
                   string refreshToken = null) : base(id)
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
        _roles = new List<AccountRole>();
        _claims = new List<AccountClaim>();
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

    /// <summary>
    /// Updates the <see cref="RefreshToken"/>
    /// </summary>
    /// <param name="refreshToken">The new refresh token</param>
    public void ChangeRefreshToken(string refreshToken)
    {
        RefreshToken = refreshToken;
    }

    /// <summary>
    /// Removes the <see cref="RefreshToken"/> previously associated with the current <see cref="Account"/>.
    /// </summary>
    public void DeleteRefreshToken() => RefreshToken = null;

    /// <summary>
    /// Adds or updates a claim in the current <see cref="Account"/> instance.
    /// </summary>
    /// <param name="type">Type of the claim.</param>
    /// <param name="value">Value of the claim</param>
    /// <param name="start">When the claim starts</param>
    /// <param name="end">When the claim ends</param>
    /// <exception cref="ArgumentNullException">if <paramref name="type"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">if <paramref name="end"/> is not <c>null</c>
    /// and <paramref name="start"/> &gt; <paramref name="end"/>
    /// </exception>
    /// <remarks>
    /// This methods will try preventing a claim to be added twice but should not be considered thread safe.
    /// </remarks>
    public void AddOrUpdateClaim(string type, string value, Instant start, Instant? end = null)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }
        if (end != default && start > end)
        {
            throw new ArgumentOutOfRangeException(nameof(start), "start > end");
        }

        Option<AccountClaim> optionClaim = _claims.SingleOrNone(ac => ac.Claim.Type == type);

        optionClaim.Match(
            some: ac => ac.ChangeValueTo(value),
            () => _claims.Add(new AccountClaim(Id, AccountClaimId.New(), type, value, start, end))
        );
    }

    /// <summary>
    /// Remove the <see cref="AccountClaim"/> with the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type of claim to remove</param>
    /// <exception cref="ArgumentNullException">if <paramref name="type"/> is <c>null</c>.</exception>
    public void RemoveClaim(string type)
    {
        if (type == default)
        {
            throw new ArgumentNullException(nameof(type));
        }
        Option<AccountClaim> optionClaim = _claims.SingleOrNone(ac => ac.Claim.Type == type);
        optionClaim.MatchSome(claim => _claims.Remove(claim));
    }

    /// <summary>
    /// Defines the owner of the current element
    /// </summary>
    /// <param name="tenantId"></param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tenantId"/> is <see cref="TenantId.Empty"/></exception>
    public void OwnsBy(TenantId tenantId)
    {
        if (tenantId == TenantId.Empty)
        {
            throw new ArgumentOutOfRangeException(nameof(tenantId), tenantId, "Tenant ID cannot be empty");
        }
        TenantId = tenantId;
    }

    /// <summary>
    /// Gives a new <see cref="Role"/> to the current account.
    /// </summary>
    /// <param name="role"></param>
    public void AddRole(Role role)
    {
        _roles.Add(new AccountRole(Id, role.Id));
    }

    /// <summary>
    /// Change the username
    /// </summary>
    /// <param name="newUsername"></param>
    public void ChangeUsernameTo(UserName newUsername) => Username = newUsername;
}
