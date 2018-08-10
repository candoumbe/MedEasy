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
    /// Execution of a <see cref="GetOneAccountByIdQuery"/> returns an <see cref="Option{AccountInfo}"/>
    /// </remarks>
    public class GetOneAccountByIdQuery : GetOneResourceQuery<Guid, Guid, Option<AccountInfo>>
    {
        /// <summary>
        /// Builds a new <see cref="GetOneAccountByIdQuery"/> instance.
        /// </summary>
        /// <param name="id">id of the resource to read</param>
        /// <exception cref="ArgumentException"><c>id == default(Guid)</c></exception>
        public GetOneAccountByIdQuery(Guid id) : base(Guid.NewGuid(), id)
        {
            if (id == default)
            {
                throw new ArgumentException($"{nameof(id)} must not be {default(Guid)}",nameof(id));
            }
        }
    }
}
