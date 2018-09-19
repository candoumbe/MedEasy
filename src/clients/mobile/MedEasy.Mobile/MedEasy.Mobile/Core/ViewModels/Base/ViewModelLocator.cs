using CommonServiceLocator;
using Unity;

namespace MedEasy.Mobile.Core.ViewModels.Base
{
    public class ViewModelLocator
    {


        static ViewModelLocator()
        {
        }

        private static SignUpViewModel _signUpViewModel;
        public static SignUpViewModel SignUpViewModel
        {
            get
            {
                if (_signUpViewModel == null)
                {
                    _signUpViewModel = ServiceLocator.Current.GetInstance<SignUpViewModel>();
                }

                return _signUpViewModel;
            }
        }


        private static SignInViewModel _signInViewModel;
        public static SignInViewModel SignInViewModel
        {
            get
            {
                if (_signInViewModel == null)
                {
                    _signInViewModel = ServiceLocator.Current.GetInstance<SignInViewModel>();
                }

                return _signInViewModel;
            }
        }

        private static LostPasswordViewModel _lostPasswordViewModel;
        public static LostPasswordViewModel LostPasswordViewModel
        {
            get
            {
                if (_lostPasswordViewModel == null)
                {
                    _lostPasswordViewModel = ServiceLocator.Current.GetInstance<LostPasswordViewModel>();
                }

                return _lostPasswordViewModel;
            }
        }
    }
}
