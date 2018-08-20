using Identity.CQRS.Commands;
using Identity.DTO;
using Identity.Objects;
using MedEasy.Abstractions;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.Interfaces;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using Optional;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.CQRS.Handlers.Commands
{
    public class HandleRefreshAccessTokenByUsernameCommand : IRequestHandler<RefreshAccessTokenByUsernameCommand, Option<BearerTokenInfo, RefreshAccessCommandResult>>
    {
        private readonly IDateTimeService _datetimeService;
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IHandleCreateSecurityTokenCommand _handleCreateSecurityTokenCommand;

        /// <summary>
        /// Builds a new <see cref="HandleRefreshAccessTokenByUsernameCommand"/> instance
        /// </summary>
        /// <param name="datetimeService">Service which gives access to current datetime</param>
        /// <param name="uowFactory"></param>
        /// <param name="handleCreateSecurityTokenCommand">Service to create <see cref="SecurityToken"/> instances.</param>
        public HandleRefreshAccessTokenByUsernameCommand(IDateTimeService datetimeService, IUnitOfWorkFactory uowFactory, IHandleCreateSecurityTokenCommand handleCreateSecurityTokenCommand)
        {
            _datetimeService = datetimeService;
            _uowFactory = uowFactory;
            _handleCreateSecurityTokenCommand = handleCreateSecurityTokenCommand;
        }

        public async Task<Option<BearerTokenInfo, RefreshAccessCommandResult>> Handle(RefreshAccessTokenByUsernameCommand cmd, CancellationToken ct)
        {
            Option<BearerTokenInfo, RefreshAccessCommandResult> optionalBearer = default;
            DateTime utcNow = _datetimeService.UtcNow();

            (string username, string expiredAccessTokenString, string refreshTokenString, JwtSecurityTokenOptions accessTokenOptions) = cmd.Data;
            JwtSecurityToken refreshToken = new JwtSecurityToken(refreshTokenString);
            JwtSecurityToken accessToken = new JwtSecurityToken(expiredAccessTokenString);

            if (refreshToken.ValidTo <= utcNow)
            {
                optionalBearer = Option.None<BearerTokenInfo, RefreshAccessCommandResult>(RefreshAccessCommandResult.Unauthorized);
            }
            else
            {

                CreateSecurityTokenCommand createNewAccessTokenCmd = new CreateSecurityTokenCommand((accessTokenOptions, accessToken.Claims.Select(claim => new ClaimInfo { Type = claim.Type, Value = claim.Value })));
                SecurityToken newAccessToken = await _handleCreateSecurityTokenCommand.Handle(createNewAccessTokenCmd, ct)
                    .ConfigureAwait(false);

                optionalBearer = Option.Some<BearerTokenInfo, RefreshAccessCommandResult>(new BearerTokenInfo { AccessToken = newAccessToken.ToString(), RefreshToken = refreshTokenString });
            }
            return optionalBearer;
        }
    }
}
