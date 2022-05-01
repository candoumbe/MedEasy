namespace Identity.CQRS.Queries.Accounts;

using Identity.DTO;
using MedEasy.ValueObjects;

using MedEasy.CQRS.Core.Queries;

using Optional;

using System;

/// <summary>
/// Issue this query to retrieve an <see cref="AccountInfo"/> by its username
/// </summary>
public class GetOneAccountInfoByUsernameQuery : QueryBase<Guid, UserName, Option<AccountInfo>>
{
    /// <summary>
    /// Builds a new <see cref="GetOneAccountByIdQuery"/> instance.
    /// </summary>
    /// <param name="username">The username of the account to lookup for.</param>
    public GetOneAccountInfoByUsernameQuery(UserName username) : base(Guid.NewGuid(), username)
    {
    }
}
