namespace MedEasy.Wasm.Services
{
    using MedEasy.Wasm.Apis.Identity;
    using MedEasy.Wasm.Apis.Identity.v2;

    using Microsoft.AspNetCore.Components.Authorization;

    using Optional;

    using Refit;

    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public class MedEasyAuthenticationStateProvider : AuthenticationStateProvider
    {

        private static readonly ClaimsIdentity Anonymous = new();
        private static readonly JwtSecurityTokenHandler Handler = new();
        private readonly IIdentityApi _identityApi;
        private readonly AuthenticationStore _authenticationStore;
        private readonly ILogger<MedEasyAuthenticationStateProvider> _logger;

        /// <summary>
        /// Builds a new <see cref="MedEasyAuthenticationStateProvider"/> instance.
        /// </summary>
        /// <param name="identityApi"></param>
        /// <param name="localStorage"></param>
        /// <param name="logger"></param>
        public MedEasyAuthenticationStateProvider(IIdentityApi identityApi, AuthenticationStore authenticationStore, ILogger<MedEasyAuthenticationStateProvider> logger)
        {
            _identityApi = identityApi;
            _authenticationStore = authenticationStore;
            _logger = logger;
        }

        ///<inheritdoc/>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            ClaimsIdentity currentUser = new();

            Option<BearerTokenModel> tokenOption = (await _authenticationStore.GetToken().ConfigureAwait(false));

            return tokenOption.Match(
                some: token =>
                {
                    _logger.LogInformation("Token found in local storage");

                    JwtSecurityToken decodedToken = Handler.ReadJwtToken(token.AccessToken.Token);

                    ClaimsIdentity identity = new(decodedToken.Claims, "MedEasy");
                    
                    _logger.LogTrace("Claims : {@Claims}", identity.Claims);
                    
                    return new AuthenticationState(new ClaimsPrincipal(identity));
                },
                none: () =>
                {
                    _logger.LogInformation("No token found in local storage");
                    return new AuthenticationState(new ClaimsPrincipal(Anonymous));

                });
        }

        /// <summary>
        /// Logs the specified account
        /// </summary>
        public async Task LogIn(LoginModel login, CancellationToken ct = default)
        {
            IApiResponse<BearerTokenModel> loginResult = await _identityApi.LogIn(login, ct);

            if (loginResult.IsSuccessStatusCode)
            {
                await _authenticationStore.SetToken(loginResult.Content, ct);

                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            }
        }

        /// <summary>
        /// Disconnects the currently connected user and remove the token stored in session storage
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task LogOut(string userName, CancellationToken ct = default)
        {
            Option<BearerTokenModel> optionToken = await _authenticationStore.GetToken(ct).ConfigureAwait(false);
            await optionToken.Match(async token =>
            {
                _logger.LogInformation("Token found in local storage");
                _logger.LogInformation("Logging out user {userName}", userName);

                await _authenticationStore.RemoveToken(ct).ConfigureAwait(false);

                await _identityApi.LogOut(userName, token.AccessToken.Token, ct).ConfigureAwait(false);

            },
            () => Task.CompletedTask);
            
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
