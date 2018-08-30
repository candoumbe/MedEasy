using FluentValidation;
using Identity.DTO;
using Identity.Validators;
using MedEasy.Companion.Core.Apis;
using MedEasy.Companion.ViewModels;
using MvvmCross;
using MvvmCross.IoC;
using MvvmCross.ViewModels;
using Refit;

namespace MedEasy.Companion.Core
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            void RegisterDependencies()
            {
                CreatableTypes()
                    .EndingWith("Service")
                    .AsInterfaces()
                    .RegisterAsLazySingleton();

                Mvx.RegisterType(() => RestService.For<ITokenApi>("localhost:5000"));
                Mvx.RegisterType(() => RestService.For<IAccountsApi>("localhost:5000/identity"));
                Mvx.RegisterType<IValidator<LoginInfo>, LoginInfoValidator>();
                Mvx.RegisterType<IValidator<NewAccountInfo>, NewAccountInfoValidator>();
            }

            RegisterDependencies();
            RegisterAppStart<SignUpViewModel>();
        }
    }
}
