namespace MedEasy.Wasm.Services;

using Blazored.LocalStorage;

using MedEasy.Wasm.Apis.Identity.v2;

using Optional;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public class AuthenticationStore
{
    private readonly ILocalStorageService _localStorage;
    public const string TokenName = "token";
    public static readonly JwtSecurityTokenHandler Handler = new();

    public AuthenticationStore(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    /// <summary>
    /// Gets the token from the local storage
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Option<BearerTokenModel>> GetToken(CancellationToken cancellationToken = default)
    {
        Option<BearerTokenModel> optionToken = (await _localStorage.GetItemAsync<BearerTokenModel>(TokenName, cancellationToken))
            .SomeNotNull();

        return optionToken;
    }

    public async Task<Option<string>> GetUserName(CancellationToken cancellationToken = default)
    {
        Option<BearerTokenModel> optionToken = await GetToken(cancellationToken);

        return optionToken.Map(token =>
        {
            JwtSecurityToken jwtToken = Handler.ReadJwtToken(token.AccessToken.Token);

            return jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value;
        });
    }

    /// <summary>
    /// Sets the token in the local storage
    /// </summary>
    /// <param name="token"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task SetToken(BearerTokenModel token, CancellationToken cancellationToken = default)
    {
        await _localStorage.SetItemAsync(TokenName, token, cancellationToken);
    }

    /// <summary>
    /// Removes the token from the local storage
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task RemoveToken(CancellationToken cancellationToken = default)
    {
        await _localStorage.RemoveItemAsync(TokenName, cancellationToken);
    }
}
