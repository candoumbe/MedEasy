using Blazor.Extensions.Storage;
using Identity.Models.Auth.v1;
using Optional;
using System.Threading.Tasks;

namespace MedEasy.Web.Accounts.Services
{
    public class TokenService : ITokenService
    {
        private const string _tokenKeyName = "token";
        private readonly SessionStorage _sessionStorage;

        public TokenService(SessionStorage sessionStorage) => _sessionStorage = sessionStorage;

        public async Task SaveToken(BearerTokenModel token) => await _sessionStorage.SetItem(_tokenKeyName, token);


        public async Task<Option<BearerTokenModel>> GetToken()
        {
            BearerTokenModel bearerTokenModel = await _sessionStorage.GetItem<BearerTokenModel>(_tokenKeyName);
            return bearerTokenModel.SomeNotNull();
        }

        public async Task DeleteToken() => await _sessionStorage.RemoveItem(_tokenKeyName);
    }
}
