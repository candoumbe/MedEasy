using Identity.Models.Auth;
using Identity.Models.Auth.v1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MedEasy.Web.Accounts.Services.Identity
{
    public interface IIdentityService
    {
        /// <summary>
        /// Connect the 
        /// </summary>
        /// <param name="login"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<BearerTokenModel> Connect(LoginModel login, CancellationToken ct = default);
    }
}
