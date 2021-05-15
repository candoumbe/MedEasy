namespace Identity.CQRS.Queries.Accounts
{
    using Identity.DTO;
    using Identity.Ids;

    using MedEasy.CQRS.Core.Queries;

    using Optional;

    using System;

    /// <summary>
    /// Query to read a <see cref="AccountInfo"/> resource by its id.
    /// </summary>
    /// <remarks>
    /// Execution of a <see cref="GetOneAccountByIdQuery"/> returns an <see cref="Option{AccountInfo}"/>
    /// </remarks>
    public class GetOneAccountByIdQuery : GetOneResourceQuery<Guid, AccountId, Option<AccountInfo>>
    {
        /// <summary>
        /// Builds a new <see cref="GetOneAccountByIdQuery"/> instance.
        /// </summary>
        /// <param name="id">id of the resource to read</param>
        /// <exception cref="ArgumentException"><c>id == default(Guid)</c></exception>
        public GetOneAccountByIdQuery(AccountId id) : base(Guid.NewGuid(), id)
        {
            if (id == default)
            {
                throw new ArgumentException($"{nameof(id)} must not be {default(AccountId)}", nameof(id));
            }
        }
    }
}
