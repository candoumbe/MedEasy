using Identity.DTO;
using Refit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MedEasy.Companion.Core.Apis
{
    /// <summary>
    /// Contract for Token REST API
    /// </summary>
    public interface ITokenApi
    {
        
        /// <summary>
        /// Gets a 
        /// </summary>
        /// <param name="loginInfo"></param>
        /// <returns></returns>
        [Post("/auth/token")]
        Task<BearerTokenInfo> SignIn([Body] LoginInfo loginInfo, CancellationToken ct);

        /// <summary>
        /// Disconnects a user
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns
        [Delete("/auth/token/{username}")]
        Task LogOut([Header("Authorization : Bearer")]string bearerToken, string username, CancellationToken ct);
    }
}
