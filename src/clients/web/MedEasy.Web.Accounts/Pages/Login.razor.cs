using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blazor.Extensions.Storage;
using Identity.Models.Auth;
using Identity.Models.Auth.v1;
using MedEasy.Web.Accounts.Services;
using MedEasy.Web.Accounts.Services.Identity;
using MedEasy.Web.Core.Components;
using Microsoft.AspNetCore.Components;

namespace MedEasy.Web.Accounts.Pages
{

    public abstract class LoginComponentBase : GenericComponentBase<LoginModel>
    {

        [Inject]
        public ITokenService TokenService { get; set; }
        
        [Inject]
        public SessionStorage SessionStorage { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IIdentityApi IdentityApi { get; set; }



        /// <summary>
        /// Connect the user
        /// </summary>
        /// <returns></returns>
        public async Task Connect()
        {
            BearerTokenModel token = await IdentityApi.Login(ViewModel)
                .ConfigureAwait(false);

            await TokenService.SaveToken(token);
        }
    }
}
