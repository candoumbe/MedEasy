using Identity.DTO;
using MedEasy.Mobile.Core.Apis;
using MedEasy.Mobile.Core.Services;
using MedEasy.Mobile.Core.ViewModels.Base;
using Optional;
using System;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace MedEasy.Mobile.Core.ViewModels
{
    /// <summary>
    /// Sign up page view model
    /// </summary>
    public class SignUpViewModel : ViewModelBase
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

        public Command SignUpCommand { get; private set; }

        public Command SignInCommand { get; private set; }

        public SignUpViewModel(IAccountsApi accountsApi, INavigatorService navigationService)
        {
            Name = string.Empty;
            Username = string.Empty;
            Password = string.Empty;

            NewAccountInfo GetNewAccountInfo(string name, string email, string username, string password, string confirmPassword) =>
                new NewAccountInfo { Name = name, Email = email, Password = password, ConfirmPassword = confirmPassword, Username = username };
            SignUpCommand = new Command(
                async () =>
                {
                    IsBusy = true;
                    Option<BearerTokenInfo> optionalTokenInfo = (await accountsApi.SignUp(GetNewAccountInfo(Name, Email, Username, Password,ConfirmPassword), default)
                            .ConfigureAwait(true)).SomeNotNull();

                    optionalTokenInfo.MatchSome(async tokenInfo =>
                    {
                        await navigationService.NavigateTo<HomeViewModel>();
                        await navigationService.RemoveBackStackAsync();
                    });
                    IsBusy = false;
                },
                canExecute: () => !(string.IsNullOrWhiteSpace(Username) 
                    || string.IsNullOrWhiteSpace(Email) 
                    || string.IsNullOrWhiteSpace(Password)
                    || string.IsNullOrWhiteSpace(ConfirmPassword)
                    )
                    && string.Equals(Password, ConfirmPassword, StringComparison.Ordinal)
                );
            SignInCommand = new Command(async () => await navigationService.NavigateTo<SignInViewModel>());
        }


        protected override void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(Username):
                case nameof(Email):
                case nameof(Password):
                case nameof(ConfirmPassword):
                    SignUpCommand?.ChangeCanExecute();
                    break;
                default:
                    break;
            }
        }
    }
}
