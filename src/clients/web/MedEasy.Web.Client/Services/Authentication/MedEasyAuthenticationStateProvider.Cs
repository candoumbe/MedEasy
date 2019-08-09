using Identity.Models.Auth;
using MedEasy.Web.Client.Services.Identity;
using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens.JWT;
using Optional;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MedEasy.Web.Client.Services.Authentication
{
    /// <summary>
    /// <see cref="AuthenticationStateProvider"/> implementation that relies on MedEasy's Identity API
    /// </summary>
    public class MedEasyAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IIdentityApi _identityApi;
        private readonly ITokenService _tokenService;

        /// <summary>
        /// Builds a new <see cref="MedEasyAuthenticationStateProvider"/> instance
        /// </summary>
        /// <param name="identityApi">client that allows to interact with Identity API</param>
        /// <param name="tokenService">Service that can save/delete token</param>
        /// <exception cref="ArgumentNullException">either <paramref name="identityApi"/> or <paramref name="tokenService"/> is <c>null</c></exception>
        public MedEasyAuthenticationStateProvider(IIdentityApi identityApi, ITokenService tokenService)
        {
            _identityApi = identityApi ?? throw new ArgumentNullException(nameof(identityApi));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {

            Option<BearerTokenModel> optionalToken = await _tokenService.GetToken();
            return optionalToken.Match(
                some: token =>
               {
                   var refreshToken = new JWTSecurityToken(token.RefreshToken);

                   return new AuthenticationState(new ClaimsPrincipal());
               },
                none: () => new AuthenticationState(new ClaimsPrincipal())
                ); ;

            ;
        }
    }
}
