using Identity.DTO;
using Refit;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MedEasy.Companion.Core.Apis
{
    public interface IAccountsApi
    {
        /// <summary>
        /// Register a new account that can then be used to interact with the api
        /// </summary>
        /// <param name="newAccount"></param>
        /// <returns></returns>

        [Post("/identity/accounts")]
        Task<BearerTokenInfo> SignUp([Body] NewAccountInfo newAccount, CancellationToken ct);

        /// <summary>
        /// Checks if any accounts exists using
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [Head("/identity/accounts")]
        Task<HttpResponseMessage> Exists(SearchAccountInfo search, CancellationToken ct);

    }
}
