using Identity.Models;
using Identity.Models.v1;
using Identity.Models.v2;
using MedEasy.RestObjects;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MedEasy.Web.Services.Identity
{
    public interface IIdentityApi
    {
        /// <summary>
        /// Register a new account
        /// </summary>
        /// <param name="register"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Post("/v1/accounts/")]
        Task<ApiResponse<BearerTokenModel>> Register([Body] RegisterModel register, CancellationToken ct = default);

        /// <summary>
        /// Logs a user in
        /// </summary>
        /// <param name="login"></param>
        /// <param name="ct"></param>
        /// <returns><see cref="ApiResponse{T}"/> with the appropriate <see cref="BearerTokenModel"/></returns>
        [Post("/v2/token")]
        Task<ApiResponse<BearerTokenModel>> Login([Body] LoginModel login, CancellationToken ct = default);

        [Get("/v1/accounts")]
        Task<ApiResponse<IEnumerable<Browsable<AccountModel>>>> Accounts([Header("Authorization")] string accessToken, [Query]int page = 1, [Query]int pageSize = 30, CancellationToken ct = default);
    }
}
