using System.Threading.Tasks;
using Identity.Models.Auth;
using Optional;

namespace MedEasy.Web.Client.Services
{
    public interface ITokenService
    {
        /// <summary>
        /// Delete the cuurre
        /// </summary>
        /// <returns></returns>
        Task DeleteToken();
        Task<Option<BearerTokenModel>> GetToken();
        Task SaveToken(BearerTokenModel token);
    }
}