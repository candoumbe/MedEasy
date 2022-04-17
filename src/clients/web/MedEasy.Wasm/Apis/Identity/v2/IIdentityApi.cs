namespace MedEasy.Wasm.Apis.Identity.v2;

using Refit;
using MedEasy.Wasm.Apis.Identity;
using MedEasy.RestObjects;

[Headers("api-version:2.0")]
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
    Task<IApiResponse<BearerTokenModel>> Refresh([Query]string username, [Authorize] string token, [Body] v1.RefreshAccessTokenModel model, CancellationToken ct = default);

    [Get("/accounts")]
    Task<IApiResponse<Page<Browsable<AccountModel>>>> GetAccounts(int page = 1, int pageSize = 10, CancellationToken ct = default);

    /// <summary>
    /// Gets <see cref="AccountModel"/>
    /// </summary>
    /// <param name="page">1-based indedx of the page of result set to get</param>
    /// <param name="pageSize">index of the page to </param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [Head("/accounts")]
    Task<IApiResponse<Page<Browsable<AccountModel>>>> GetAccountsHead([Query] int page = 1, [Query]int pageSize = 10, CancellationToken ct = default);

    /// <summary>
    /// Updates the specified account
    /// </summary>
    /// <param name="id">identifier of the resource to update</param>
    /// <param name="operations">set of operations to apply</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [Patch("/accounts/{id}")]
    [Headers("Content-Type: application/json+patch")]
    Task<IApiResponse<Browsable<AccountModel>>> Patch(Guid id, [Body] IEnumerable<PatchOperation> operations, CancellationToken ct = default);
}


