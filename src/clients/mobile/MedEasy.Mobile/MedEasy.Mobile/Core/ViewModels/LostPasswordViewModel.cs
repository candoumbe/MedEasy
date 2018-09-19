using System.Threading.Tasks;
using MedEasy.Mobile.Core.Services;
using MedEasy.Mobile.Core.ViewModels.Base;
using Xamarin.Forms;

namespace MedEasy.Mobile.Core.ViewModels
{

    public class LostPasswordViewModel : ViewModelBase
    {
        private string _email;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value, onChanged: () => ResetPasswordCommand.ChangeCanExecute());
        }


        public Command ResetPasswordCommand { get; }

        /// <summary>
        /// Builds a new <see cref="LostPasswordViewModel"/> instance
        /// </summary>
        /// <param name="navigatorService"></param>
        public LostPasswordViewModel(INavigatorService navigatorService)
        {
            ResetPasswordCommand = new Command(
                execute: async () => await navigatorService.PopModalAsync(),
                canExecute: () => !string.IsNullOrWhiteSpace(Email)
            );
        }
    }
}
