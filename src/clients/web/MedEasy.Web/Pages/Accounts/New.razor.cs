
using Identity.Models.v1;

using Microsoft.AspNetCore.Components;

using System.Net.Http;
using System.Threading.Tasks;

namespace MedEasy.Web.Pages.Accounts
{
    public partial class New
    {
        public NewAccountModel NewAccount = new();

        [Inject]
        public HttpClient HttpClient { get; set; }

        public bool IsLoading { get; set; }

        public async Task Register()
        {
            IsLoading = true;

            await Task.Delay(5_000);

            IsLoading = false;
        }

        public bool CanRegister() => !(string.IsNullOrWhiteSpace(NewAccount.Email) 
                                       || string.IsNullOrWhiteSpace(NewAccount.Password))
            ;


    }
}
