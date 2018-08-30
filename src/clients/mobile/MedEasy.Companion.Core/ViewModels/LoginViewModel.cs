using FluentValidation;
using Identity.DTO;
using MedEasy.Companion.Core.Apis;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Optional;
using System.Threading.Tasks;


namespace MedEasy.Companion.ViewModels
{
    /// <summary>
    /// Login page view model
    /// </summary>
    public class LoginViewModel : MvxViewModel
    {
        private string _login;

        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }


        private string _password;

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }


        public MvxAsyncCommand SignInCommand { get; private set; }
        public MvxCommand SignUpCommand { get; private set; }

        public LoginViewModel(ITokenApi tokenApi, IValidator<LoginInfo> validator, IMvxNavigationService navigationService)
        {
            Login = string.Empty;
            Password = string.Empty;
            NavigationService = navigationService;

            LoginInfo GetLoginInfo(string login, string password) => new LoginInfo { Username = login, Password = password };
            SignInCommand = new MvxAsyncCommand(
                async (ct) =>
                {
                    Option<BearerTokenInfo> optionalTokenInfo = (await tokenApi.SignIn(GetLoginInfo(Login, Password), ct)
                            .ConfigureAwait(true)).SomeNotNull();

                    optionalTokenInfo.MatchSome(tokenInfo =>
                    {
                        
                    });
                },
                canExecute: () => validator.Validate(GetLoginInfo(Login, Password)).IsValid
                );

            SignUpCommand = new MvxCommand(() => NavigationService.Navigate<SignUpViewModel>());
        }
    }
}
