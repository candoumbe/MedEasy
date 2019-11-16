using Identity.Models.Accounts;
using Identity.Models.Auth.v1;
using MedEasy.Web.Accounts.Services;
using MedEasy.Web.Accounts.Services.Identity;
using MedEasy.Web.Core.Components;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace MedEasy.Web.Accounts.Pages
{
    public abstract class RegisterComponentBase : GenericComponentBase<NewAccountModel>
    {

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IIdentityApi IdentityApi { get; set; }

        [Inject]
        public ITokenService TokenService { get; set; }



        public async Task CreateAccount()
        {
            BearerTokenModel token = await IdentityApi.Register(ViewModel, default);
            await TokenService.SaveToken(token);
            NavigationManager.NavigateTo("/");
        }
    }
}
