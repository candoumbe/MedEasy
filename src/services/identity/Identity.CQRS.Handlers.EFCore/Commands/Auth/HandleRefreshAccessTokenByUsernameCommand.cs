namespace Identity.CQRS.Handlers.Commands
{
    using Identity.CQRS.Commands;
    using Identity.DTO;
    using Identity.DTO.v1;
    using Identity.Objects;
    using MedEasy.ValueObjects;

    using MedEasy.CQRS.Core.Commands.Results;
    using MedEasy.DAL.Interfaces;

    using MediatR;

    using Microsoft.IdentityModel.Tokens;

    using NodaTime;

    using Optional;

    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handle <see cref=""/>
    /// </summary>
    public class HandleRefreshAccessTokenByUsernameCommand : IRequestHandler<RefreshAccessTokenByUsernameCommand, Option<BearerTokenInfo, RefreshAccessCommandResult>>
    {
        private readonly IClock _datetimeService;
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IHandleCreateSecurityTokenCommand _handleCreateSecurityTokenCommand;

        /// <summary>
        /// Builds a new <see cref="HandleRefreshAccessTokenByUsernameCommand"/> instance
        /// </summary>
        /// <param name="datetimeService">Service which gives access to current datetime</param>
        /// <param name="uowFactory"></param>
        /// <param name="handleCreateSecurityTokenCommand">Service to create <see cref="SecurityToken"/> instances.</param>
        public HandleRefreshAccessTokenByUsernameCommand(IClock datetimeService, IUnitOfWorkFactory uowFactory, IHandleCreateSecurityTokenCommand handleCreateSecurityTokenCommand)
        {
            _datetimeService = datetimeService;
            _uowFactory = uowFactory;
            _handleCreateSecurityTokenCommand = handleCreateSecurityTokenCommand;
        }

        public async Task<Option<BearerTokenInfo, RefreshAccessCommandResult>> Handle(RefreshAccessTokenByUsernameCommand cmd, CancellationToken ct)
        {
            Option<BearerTokenInfo, RefreshAccessCommandResult> optionalBearer = default;
            Instant utcNow = _datetimeService.GetCurrentInstant();

            (UserName username, string expiredAccessTokenString, string refreshTokenString, JwtInfos tokenOptions) = cmd.Data;
            JwtSecurityToken refreshToken = new(refreshTokenString);
            JwtSecurityToken accessToken = new(expiredAccessTokenString);

            if (refreshToken.ValidFrom > utcNow.ToDateTimeUtc() || refreshToken.ValidTo <= utcNow.ToDateTimeUtc())
            {
                optionalBearer = Option.None<BearerTokenInfo, RefreshAccessCommandResult>(RefreshAccessCommandResult.Unauthorized);
            }
            else
            {
                using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
                Option<Account> optionalAccount = await uow.Repository<Account>().SingleOrDefaultAsync(x => x.RefreshToken == refreshTokenString && x.Username == username, ct)
                    .ConfigureAwait(false);
                optionalBearer = await optionalAccount.Match<ValueTask<Option<BearerTokenInfo, RefreshAccessCommandResult>>>(
                    some: async account =>
                    {
                        JwtSecurityTokenOptions accessTokenOptions = new()
                        {
                            Audiences = tokenOptions.Audiences,
                            Issuer = tokenOptions.Issuer,
                            Key = tokenOptions.Key,
                            LifetimeInMinutes = tokenOptions.AccessTokenLifetime
                        };
                        CreateSecurityTokenCommand createNewAccessTokenCmd = new((accessTokenOptions, utcNow, accessToken.Claims.Select(claim => new ClaimInfo { Type = claim.Type, Value = claim.Value })));
                        Task<SecurityToken> newAccessTokenTask = _handleCreateSecurityTokenCommand.Handle(createNewAccessTokenCmd, ct);

                        JwtSecurityTokenOptions refreshTokenOptions = new()
                        {
                            Audiences = tokenOptions.Audiences,
                            Issuer = tokenOptions.Issuer,
                            Key = tokenOptions.Key,
                            LifetimeInMinutes = tokenOptions.RefreshTokenLifetime
                        };
                        CreateSecurityTokenCommand createNewRefreshTokenCmd = new((refreshTokenOptions, utcNow, accessToken.Claims.Select(claim => new ClaimInfo { Type = claim.Type, Value = claim.Value })));
                        Task<SecurityToken> newRefreshTokenTask = _handleCreateSecurityTokenCommand.Handle(createNewRefreshTokenCmd, ct);

                        await Task.WhenAll(newAccessTokenTask, newRefreshTokenTask)
                                  .ConfigureAwait(false);

                        account.ChangeRefreshToken((await newRefreshTokenTask).ToString());

                        await uow.SaveChangesAsync(ct)
                            .ConfigureAwait(false);

                        return Option.Some<BearerTokenInfo, RefreshAccessCommandResult>(new BearerTokenInfo { AccessToken = (await newAccessTokenTask).ToString(), RefreshToken = refreshTokenString });
                    },
                    none: () => new ValueTask<Option<BearerTokenInfo, RefreshAccessCommandResult>>(Option.None<BearerTokenInfo, RefreshAccessCommandResult>(RefreshAccessCommandResult.Unauthorized))
                )
                .ConfigureAwait(false);
            }
            return optionalBearer;
        }
    }
}
