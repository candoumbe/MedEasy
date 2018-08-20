using Identity.DTO;
using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;
using Optional;
using System;

namespace Identity.CQRS.Commands
{
    /// <summary>
    /// Command to invalidate an access token for an account.
    /// </summary>
    public class RefreshAccessTokenByUsernameCommand : CommandBase<Guid, (string username, string expiredAccessToken, string refreshToken, JwtSecurityTokenOptions accessTokenOptions), Option<BearerTokenInfo, RefreshAccessCommandResult>>
    {
        /// <summary>
        /// Builds a new <see cref="InvalidateAccessTokenByUsernameCommand"/> instance
        /// </summary>
        /// <param name="data"></param>
        public RefreshAccessTokenByUsernameCommand((string username, string expiredAccessToken, string refreshToken, JwtSecurityTokenOptions accessTokenOptions)  data) : base(Guid.NewGuid(), data)
        {
            if (string.IsNullOrWhiteSpace(data.username))
            {
                throw new ArgumentException(nameof(data.username), $"{nameof(data.username)} is null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(data.expiredAccessToken))
            {
                throw new ArgumentException(nameof(data.expiredAccessToken), $"{nameof(data.expiredAccessToken)} is null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(data.refreshToken))
            {
                throw new ArgumentException(nameof(data.refreshToken), $"{nameof(data.refreshToken)} is null or whitespace");
            }

            if (data.accessTokenOptions == default)
            {
                throw new ArgumentException(nameof(data.accessTokenOptions), $"{nameof(data.accessTokenOptions)} is null");
            }
        }
    }
}
