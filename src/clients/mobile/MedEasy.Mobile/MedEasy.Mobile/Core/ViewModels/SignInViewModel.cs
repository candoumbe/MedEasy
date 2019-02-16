using Identity.DTO;
using MedEasy.Mobile.Core.Apis;
using MedEasy.Mobile.Core.Services;
using MedEasy.Mobile.Core.ViewModels.Base;
using MedEasy.Mobile.Core.ViewModels.NavigationData;
using Optional;
using Xamarin.Forms;

namespace MedEasy.Mobile.Core.ViewModels
{
    /// <summary>
    /// Login page view model
    /// </summary>
    public class SignInViewModel : ViewModelBase
    {
        private string _login;

        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value, onChanged: () => SignInCommand.ChangeCanExecute());
        }


        private string _password;

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value, onChanged: () => SignInCommand.ChangeCanExecute());
        }


        public Command SignInCommand { get; private set; }
        public Command LostPasswordCommand { get; private set; }

        public SignInViewModel(INavigatorService navigationService, ITokenApi tokenApi)
        {
            LoginInfo GetLoginInfo(string login, string password) => new LoginInfo { Username = login, Password = password };
            SignInCommand = new Command(
                async () =>
                {
                    Option<BearerTokenInfo> optionalTokenInfo = (await tokenApi.SignIn(GetLoginInfo(Login, Password), default)
                            .ConfigureAwait(true)).SomeNotNull();

                    optionalTokenInfo.MatchSome(async tokenInfo =>
                    {
                        navigationService.InsertViewModelBefore<HomeViewModel, SignInViewModel>();
                        await navigationService.NavigateTo<HomeViewModel>(animated: true);

                    });
                },
                canExecute: () => !(string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password)));

            LostPasswordCommand = new Command(async () => await navigationService.PushModalAsync<LostPasswordViewModel>(new LostPasswordNavigationData { Email = Login }));

            Login = string.Empty;
            Password = string.Empty;

        }
    }
}
