using Identity.DTO;
using Identity.DTO.Auth;
using Identity.DTO.v1;
using Identity.Ids;

using MedEasy.RestObjects;

using Refit;

using System.Threading;
using System.Threading.Tasks;

namespace Identity.Api.Refit.v1;

/// <summary>
/// Authentication API v1
/// </summary>
public interface IAuthApi
{
    /// <summary>
    /// Connects the user with the specified <paramref name="login"/>
    /// </summary>
    /// <param name="login">Contains username and password of the user to authenticate</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [Post("/v1/auth/token")]
    Task<ApiResponse<BearerTokenInfo>> Login([Body]LoginInfo login, CancellationToken ct);

    /// <summary>
    /// Disconnects the user with the specified <see cref="username"/>
    /// </summary>
    /// <param name="login">Contains username and password of the user to authenticate</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [Delete("/v1/auth/token/{username}")]
    Task Disconnect(string username, CancellationToken ct);

    /// <summary>
    /// Ask for a new refresh token for the user with the specified name.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="refresh"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [Put("/v1/auth/token/{username}")]
    Task<ApiResponse<BearerTokenInfo>> Refresh(string username, [Body] RefreshAccessTokenInfo refresh, CancellationToken ct);
}
