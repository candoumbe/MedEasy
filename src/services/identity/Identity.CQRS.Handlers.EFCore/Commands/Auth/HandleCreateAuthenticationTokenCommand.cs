namespace Identity.CQRS.Handlers.EFCore.Commands.Auth
{
    using Identity.CQRS.Commands;
    using Identity.DTO;
    using Identity.Objects;

    using MedEasy.DAL.Interfaces;

    using MediatR;

    using Microsoft.IdentityModel.Tokens;

    using NodaTime;

    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles creation of token suitable for authenticating an <see cref="AccountInfo"/>.
    /// </summary>
    public class HandleCreateAuthenticationTokenCommand : IRequestHandler<CreateAuthenticationTokenCommand, AuthenticationTokenInfo>
    {
        private readonly IClock _dateTimeService;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IHandleCreateSecurityTokenCommand _handleCreateSecurityTokenCommand;

        /// <summary>
        /// Builds a new <see cref="HandleCreateAuthenticationTokenCommand"/> instance.
        /// </summary>
        /// <param name="dateTimeService">Service that provide methods to get current date.</param>
        /// <param name="unitOfWorkFactory"></param>
        /// <param name="handleCreateSecurityTokenCommand"></param>
        /// 
        public HandleCreateAuthenticationTokenCommand(IClock dateTimeService, IUnitOfWorkFactory unitOfWorkFactory, IHandleCreateSecurityTokenCommand handleCreateSecurityTokenCommand)
        {
            _dateTimeService = dateTimeService;
            _unitOfWorkFactory = unitOfWorkFactory;
            _handleCreateSecurityTokenCommand = handleCreateSecurityTokenCommand;
        }

        public async Task<AuthenticationTokenInfo> Handle(CreateAuthenticationTokenCommand cmd, CancellationToken ct)
        {
            Instant now = _dateTimeService.GetCurrentInstant();
            (AuthenticationInfo authInfo, AccountInfo accountInfo, JwtInfos jwtInfos) = cmd.Data;

            IEnumerable<string> audiences = jwtInfos.Audiences?.Distinct() ?? Enumerable.Empty<string>();

            IEnumerable<ClaimInfo> refreshTokenClaims = new[]
            {
                    new ClaimInfo{ Type = JwtRegisteredClaimNames.Jti, Value =  Guid.NewGuid().ToString() },
                    new ClaimInfo{ Type = CustomClaimTypes.AccountId, Value= accountInfo.Id.ToString() },
                    new ClaimInfo{ Type = ClaimTypes.Name, Value = accountInfo.Name ?? accountInfo.Username },
                    new ClaimInfo{ Type = ClaimTypes.NameIdentifier, Value = accountInfo.Username },
                    new ClaimInfo{ Type = ClaimTypes.Email, Value = accountInfo.Email},
                    new ClaimInfo{ Type = ClaimTypes.GivenName, Value = accountInfo.Name ?? accountInfo.Username },
                    new ClaimInfo{ Type = CustomClaimTypes.Location, Value = authInfo.Location},
                    new ClaimInfo{ Type = CustomClaimTypes.TimeZoneId, Value = DateTimeZone.Utc.Id }
            }.Union(audiences.Select(audience => new ClaimInfo { Type = JwtRegisteredClaimNames.Aud, Value = audience }));

            IEnumerable<ClaimInfo> accessTokenClaims = refreshTokenClaims.Union(accountInfo.Claims);

            SecurityKey signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtInfos.Key));

            JwtSecurityTokenOptions jwtAccessTokenOptions = new()
            {
                Issuer = jwtInfos.Issuer,
                Audiences = jwtInfos.Audiences,
                Key = jwtInfos.Key,
                LifetimeInMinutes = jwtInfos.AccessTokenLifetime
            };

            CreateSecurityTokenCommand createAccessTokenCommand = new((jwtAccessTokenOptions, accessTokenClaims));
            Task<SecurityToken> accessTokenTask = _handleCreateSecurityTokenCommand.Handle(createAccessTokenCommand, ct);

            JwtSecurityTokenOptions jwtRefreshTokenOptions = new()
            {
                Issuer = jwtInfos.Issuer,
                Audiences = jwtInfos.Audiences,
                Key = jwtInfos.Key,
                LifetimeInMinutes = jwtInfos.RefreshTokenLifetime
            };
            CreateSecurityTokenCommand createRefreshTokenCommand = new((jwtRefreshTokenOptions, refreshTokenClaims));
            Task<SecurityToken> refreshTokenTask = _handleCreateSecurityTokenCommand.Handle(createRefreshTokenCommand, ct);

            await Task.WhenAll(accessTokenTask, refreshTokenTask)
                      .ConfigureAwait(false);

            using IUnitOfWork uow = _unitOfWorkFactory.NewUnitOfWork();
            Account authenticatedAccount = await uow.Repository<Account>()
                                                    .SingleAsync(x => x.Id == accountInfo.Id, ct)
                                                    .ConfigureAwait(false);

            SecurityToken accessToken = await accessTokenTask;
            SecurityToken refreshToken = await refreshTokenTask;

            authenticatedAccount.ChangeRefreshToken(refreshToken.ToString());

            await uow.SaveChangesAsync(ct)
                     .ConfigureAwait(false);

            return new AuthenticationTokenInfo
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
    }
}
