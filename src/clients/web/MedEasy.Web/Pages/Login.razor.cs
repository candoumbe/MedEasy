using Blazored.LocalStorage;
using Blazorise;
using Identity.Models;
using Identity.Models.v2;

using MedEasy.Web.Services.Identity;

using Microsoft.AspNetCore.Components;

using Refit;

using System.Threading.Tasks;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace MedEasy.Web.Pages
{
    public class LoginBase : ComponentBase
    {
        [Inject]
        public NavigationManager NavigationService { get; set; }

        [Inject]
        public IIdentityApi Loginservice { get; set; }

        [Inject]
        public ILocalStorageService LocalStorageService { get; set; }

        public LoginModel Model { get; set; }

        public bool IsConnecting { get; set; }

        /// <summary>
        /// Indicates if the Connect button is disabled.
        /// </summary>
        public bool CanConnect { get; set; }

        public string ErrorMessage { get; set; }

        public Alert ErrorComponent { get; set; }
        
        public LoginBase()
        {
            Model = new LoginModel();
        }

        /// <summary>
        /// Attemps to log into the app with specified credentials.
        /// </summary>
        /// <returns></returns>
        public async Task Connect()
        {
            IsConnecting = true;
            ErrorComponent.Hide();
            StateHasChanged();
            try
            {
                ApiResponse<BearerTokenModel> response = await Loginservice.Login(Model);

                if (response.IsSuccessStatusCode)
                {
                    await LocalStorageService.SetItemAsync("bearer", response.Content);
                    NavigationService.NavigateTo("/home");
                }
                else
                {
                    IsConnecting = false;
                    ErrorMessage = ((int)response.StatusCode) switch
                    {
                        Status404NotFound => "Incorrect Username or password",
                        _ => "An unexpected error occurred while sending the request"
                    };

                    ErrorComponent.Show();
                }
                StateHasChanged();
            }
            catch (System.Exception ex)
            {
                IsConnecting = false;
                ErrorMessage = ex.Message;
                ErrorComponent.Show();
            }
        }

        /// <summary>
        /// Navigates to the page where the user can register himself.
        /// </summary>
        public void GoToRegister() => NavigationService.NavigateTo("/register");
    }
}
