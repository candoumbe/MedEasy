using Identity.DTO;
using MedEasy.CQRS.Core.Queries;
using Optional;
using System;

namespace Identity.CQRS.Queries.Accounts
{
    /// <summary>
    /// Query to read a <see cref="AccountInfo"/> resource by its id.
    /// </summary>
    /// <remarks>
    /// Execution of a <see cref="GetAccountInfoByIdQuery"/> returns an <see cref="Option{AccountInfo}"/>
    /// </remarks>
    public class GetAccountInfoByIdQuery : GetOneResourceQuery<Guid, Guid, Option<AccountInfo>>
    {
        /// <summary>
        /// Builds a new <see cref="GetAccountInfoByIdQuery"/> instance.
        /// </summary>
        /// <param name="id">id of the resource to read</param>
        /// <exception cref="ArgumentException"><c>id == default(Guid)</c></exception>
        public GetAccountInfoByIdQuery(Guid id) : base(Guid.NewGuid(), id)
        {
            if (id == default(Guid))
            {
                throw new ArgumentException($"{nameof(id)} must not be {default(Guid)}",nameof(id));
            }
        }
    }
}
