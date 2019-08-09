using Blazor.Extensions.Storage;
using Identity.Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.Web.Client.Services
{
    public class TokenService : ITokenService
    {
        private const string TOKEN = "token";
        private readonly SessionStorage _sessionStorage;

        public TokenService(SessionStorage sessionStorage) => _sessionStorage = sessionStorage;

        public async Task SaveToken(BearerTokenModel token) => await _sessionStorage.SetItem(TOKEN, token);


        public async Task<BearerTokenModel> GetToken() => await _sessionStorage.GetItem<BearerTokenModel>(TOKEN);


        public async Task DeleteToken() => await _sessionStorage.RemoveItem(TOKEN);
    }
}
