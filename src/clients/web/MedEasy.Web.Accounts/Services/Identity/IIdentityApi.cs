using Identity.Models.Accounts;
using Identity.Models.Auth;
using Identity.Models.Auth.v1;
using Refit;
using System.Threading;
using System.Threading.Tasks;

namespace MedEasy.Web.Accounts.Services.Identity
{
    public interface IIdentityApi
    {
        /// <summary>
        /// Request a new token
        /// </summary>
        /// <param name="login"></param>
        /// <param name="ct"></param>
        /// <returns>The created token</returns>
        [Post("/auth/token")]
        Task<BearerTokenModel> Login([Body] LoginModel login, CancellationToken ct = default);

        /// <summary>
        /// Disconnect the specified sued
        /// </summary>
        /// <param name="username"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Delete("/token/{username}")]
        Task LogOut([Header("Authorization")]string bearer, string username, CancellationToken ct = default);

        /// <summary>
        /// Generate a new access token for the specified <paramref name="username"/>.
        /// </summary>
        /// <param name="bearer"></param>
        /// <param name="username"></param>
        /// <param name="token"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Put("/token/{username}")]
        Task<BearerTokenModel> RefreshToken([Header("Authorization")]string bearer, string username, BearerTokenModel token, CancellationToken ct = default);

        /// <summary>
        /// Register a new account
        /// </summary>
        /// <param name="newAccount"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Post("/accounts/")]
        Task<BearerTokenModel> Register([Body] NewAccountModel newAccount, CancellationToken ct = default);
    }
}
