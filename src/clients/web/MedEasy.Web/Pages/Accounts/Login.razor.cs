namespace MedEasy.Web.Pages.Accounts
{
    using BlazorStorage.Interfaces;

    using Identity.Models.v1;

    using MedEasy.Web.Apis.Identity.Interfaces;
    using MedEasy.Web.Constants;

    using Microsoft.AspNetCore.Components;

    using Refit;

    using System;
    using System.Threading.Tasks;

    public partial class Login
    {
        [Parameter]
        public LoginModel LoginInfo { get; set; }
        
        public bool IsLoading { get; private set; }

        private readonly ISessionStorage _storage;
        private readonly IIdentityApi _identityApi;
        private readonly NavigationManager _navigationManager;

        public Login(ISessionStorage storage, IIdentityApi identityApi, NavigationManager navigationManager)
        {
            _storage = storage;
            _identityApi = identityApi;
            _navigationManager = navigationManager;
            IsLoading = false;
            LoginInfo = new();
        }

        /// <summary>
        /// Defines is the form can be submitted in its current state
        /// </summary>
        /// <returns><c>true</c> when the form can be submitted and <c>false</c> otherwize</returns>
        public bool CanSubmit() => !(string.IsNullOrWhiteSpace(LoginInfo.Username) || string.IsNullOrWhiteSpace(LoginInfo.Password));

        /// <summary>
        /// Requests a token for the current <see cref="LoginInfo"/>
        /// </summary>
        /// <remarks>
        /// This method will try authenticate against <c>Identity.API</c> and, when successful, the JWT token will be stored
        /// under <see cref="Constants.Storage.Token"/>.
        /// </remarks>
        public async Task Connect()
        {
            IsLoading = true;
            ApiResponse<BearerTokenModel> apiResponse = await _identityApi.Connect(LoginInfo, default)
                                                                          .ConfigureAwait(false);

            if (apiResponse.IsSuccessStatusCode)
            {
                await _storage.SetItem(StorageKeyNames.Token, apiResponse.Content)
                              .ConfigureAwait(false);

                _navigationManager.NavigateTo("/");
            }

            IsLoading = false;
        }
    }
}
