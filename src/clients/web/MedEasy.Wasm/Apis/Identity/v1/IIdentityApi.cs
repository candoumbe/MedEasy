namespace MedEasy.Wasm.Apis.Identity.v1;

using MedEasy.Wasm.Apis.Identity;

using Refit;

[Headers("api-version:1.0")]
public interface IIdentityApi
{
    /// <summary>
    /// Gets a <see cref="BearerTokenModel"/> for the specified login
    /// </summary>
    /// <param name="login"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [Post("/auth/token")]
    Task<IApiResponse<BearerTokenModel>> LogIn([Body] LoginModel login, CancellationToken ct = default);

    /// <summary>
    /// Disconnects the specified account
    /// </summary>
    /// <param name="login"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [Delete("/auth/token/{username}")]
    Task<IApiResponse> LogOut([Query] string username, [Authorize] string token, CancellationToken ct = default);

    /// <summary>
    /// Asks a new access token for <paramref name="username"/>
    /// </summary>
    /// <param name="username">The username which access token should be "refreshed"</param>
    /// <param name="model">wrapper of the expired access token and the refresh token to used</param>
    /// <param name="ct"></param>
    /// <returns>a new <see cref="BearerTokenModel"/> if the call is successfull</returns>
    [Put("/auth/token/{username}")]
    Task<IApiResponse<BearerTokenModel>> Refresh(string username, [Authorize] string token, [Body] RefreshAccessTokenModel model, CancellationToken ct = default);
}