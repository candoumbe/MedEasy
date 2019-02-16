using CommonServiceLocator;
using MedEasy.Mobile.Core.Apis;
using MedEasy.Mobile.Core.Services;
using MedEasy.Mobile.Core.ViewModels;
using MedEasy.Mobile.Views;
using Refit;
using System;
using Unity;
using Unity.Lifetime;
using Unity.ServiceLocation;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace MedEasy.Mobile.Core.Bootstraping
{
    public class Bootstrapper
    {
        private static UnityContainer _container;

        static Bootstrapper() => _container = new UnityContainer();

        public static void Initialize(App app)
        {

            IViewFactory viewFactory = new ViewFactory(_container);
            RegisterViews(viewFactory);

            _container.RegisterInstance<IViewFactory>(viewFactory, new SingletonLifetimeManager());
            _container.RegisterInstance(new Lazy<INavigation>(() => app.MainPage.Navigation));
            _container.RegisterType<INavigatorService, NavigatorService>();


            _container.RegisterInstance<IAccountsApi>(RestService.For<IAccountsApi>("http://localhost:5000"));
            _container.RegisterInstance<ITokenApi>(RestService.For<ITokenApi>("http://localhost:5000"));

            _container.RegisterSingleton<IApplicationService, ApplicationService>();

            ServiceLocator.SetLocatorProvider(() => new UnityServiceLocator(_container));
            DependencyResolver.ResolveUsing(type => _container.IsRegistered(type) ? _container.Resolve(type) : null);



        }

        private static void RegisterViews(IViewFactory viewFactory)
        {
            viewFactory.AddMapping<SignUpViewModel, SignUpPage>();
            viewFactory.AddMapping<SignInViewModel, SignInPage>();
            viewFactory.AddMapping<HomeViewModel, HomePage>();
            viewFactory.AddMapping<LostPasswordViewModel, LostPasswordPage>();
        }
    }
}
