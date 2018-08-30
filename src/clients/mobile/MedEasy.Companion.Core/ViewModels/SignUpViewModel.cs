using FluentValidation;
using Identity.DTO;
using MedEasy.Companion.Core.Apis;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Optional;


namespace MedEasy.Companion.ViewModels
{
    /// <summary>
    /// Sign up page view model
    /// </summary>
    public class SignUpViewModel : MvxViewModel
    {
        private string _name;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }


        private string _username;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _email;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email ,value);
        }



        private string _password;

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _confirmPassword;

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public MvxAsyncCommand SignUpCommand { get; private set; }

        public MvxCommand SignInCommand { get; private set; }

        public SignUpViewModel(IAccountsApi accountsApi, IValidator<NewAccountInfo> validator, IMvxNavigationService navigationService)
        {
            Name = string.Empty;
            Username = string.Empty;
            Password = string.Empty;
            NavigationService = navigationService;

            NewAccountInfo GetNewAccountInfo(string name, string email, string username, string password, string confirmPassword) =>
                new NewAccountInfo { Name = name, Email = email, Password = password, ConfirmPassword = confirmPassword, Username = username };
            SignUpCommand = new MvxAsyncCommand(
                async (ct) =>
                {
                    Option<BearerTokenInfo> optionalTokenInfo = (await accountsApi.SignUp(GetNewAccountInfo(Name, Email, Username, Password,ConfirmPassword), ct)
                            .ConfigureAwait(true)).SomeNotNull();

                    optionalTokenInfo.MatchSome(tokenInfo =>
                    {

                    });
                },
                canExecute: () => validator.Validate(GetNewAccountInfo(Name, Email, Username, Password, ConfirmPassword)).IsValid
                );
            SignInCommand = new MvxCommand(() => NavigationService.Navigate<LoginViewModel>());
        }
    }
}
