using Identity.Models.Auth.v1;
using Optional;
using System.Threading.Tasks;

namespace MedEasy.Web.Accounts.Services
{
    public interface ITokenService
    {
        /// <summary>
        /// Delete the token
        /// </summary>
        /// <returns></returns>
        Task DeleteToken();

        /// <summary>
        /// Gets a <see cref="BearerTokenModel"/> instance that was previously saved when calling <see cref="SaveToken(BearerTokenModel)"/>.
        /// </summary>
        /// <returns>The token</returns>
        Task<Option<BearerTokenModel>> GetToken();
       
        /// <summary>
        /// Registers <see cref="BearerTokenModel"/> so that it can be later retrieved using <see cref="GetToken"/> method
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task SaveToken(BearerTokenModel token);
    }
}