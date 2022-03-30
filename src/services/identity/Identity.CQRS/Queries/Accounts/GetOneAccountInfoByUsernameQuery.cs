namespace Identity.CQRS.Queries.Accounts;

using Identity.DTO;

using MedEasy.CQRS.Core.Queries;

using Optional;

using System;

/// <summary>
/// Issue this query to retrieve an <see cref="AccountInfo"/> by its username
/// </summary>
public class GetOneAccountInfoByUsernameQuery : QueryBase<Guid, string, Option<AccountInfo>>
{
    /// <summary>
    /// Builds a new <see cref="GetOneAccountByIdQuery"/> instance.
    /// </summary>
    /// <param name="username">The username of the account to lookup for.</param>
    public GetOneAccountInfoByUsernameQuery(string username) : base(Guid.NewGuid(), username)
    {
    }
}
