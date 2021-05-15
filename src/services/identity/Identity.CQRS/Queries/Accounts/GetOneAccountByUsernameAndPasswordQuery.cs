namespace Identity.CQRS.Queries.Accounts
{
    using MedEasy.CQRS.Core.Queries;
    using Identity.DTO;
    using Optional;
    using System;

    /// <summary>
    /// Query to retrieve a user by its <see cref="LoginInfo.Username"/> and <see cref="LoginInfo.Password"/>.
    /// </summary>
    public class GetOneAccountByUsernameAndPasswordQuery : GetOneResourceQuery<Guid, LoginInfo, Option<AccountInfo>>
    {
        /// <summary>
        /// Builds a new <see cref="GetOneAccountByUsernameAndPasswordQuery"/> instance
        /// </summary>
        /// <param name="data"></param>
        public GetOneAccountByUsernameAndPasswordQuery(LoginInfo data) : base(Guid.NewGuid(), data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
        }
    }
}
