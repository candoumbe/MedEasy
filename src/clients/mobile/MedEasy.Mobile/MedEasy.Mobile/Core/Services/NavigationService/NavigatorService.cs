using MedEasy.Mobile.Core.ViewModels;
using MedEasy.Mobile.Core.ViewModels.Base;
using Optional;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MedEasy.Mobile.Core.Services
{
    /// <summary>
    /// <see cref="INavigatorService"/> implementation which lean on Xamarin's form <see cref="INavigation"/>
    /// </summary>
    public class NavigatorService : INavigatorService
    {
        private readonly IViewFactory _viewFactory;
        private readonly Lazy<INavigation> _navigationService;
        private readonly IApplicationService _applicationService;

        /// <summary>
        /// Builds a new <see cref="NavigatorService"/> instance.
        /// </summary>
        /// <param name="viewFactory"></param>
        /// <param name="navigationService"></param>
        public NavigatorService(IViewFactory viewFactory, Lazy<INavigation> navigationService, IApplicationService applicationService)
        {
            _viewFactory = viewFactory;
            _navigationService = navigationService;
            _applicationService = applicationService;
        }

        private INavigation NavigationService => _navigationService.Value;

        public async Task NavigateTo<TViewModel>() where TViewModel : class, IViewModel
            => await NavigateTo<TViewModel>(animated: true)
            .ConfigureAwait(false);

        public async Task NavigateTo<TViewModel>(bool animated) where TViewModel : class, IViewModel
            => await PushAsync<TViewModel>(animated)
            .ConfigureAwait(false);

        public async Task PushAsync<TViewModel>(bool animated)
            where TViewModel : class, IViewModel
        {
            Option<Page> optionalPage = _viewFactory.Resolve<TViewModel>();
            await optionalPage.Match(
                some: async page =>
                {
                    if (typeof(TViewModel) == typeof(SignInViewModel))
                    {
                        _applicationService.MainPage = new NavigationPage(page);
                    }
                    else
                    {
                        await NavigationService.PushAsync(page, animated)
                            .ConfigureAwait(false);
                    }
                },
                none: () => Task.CompletedTask
            )
            .ConfigureAwait(false);
        }

        public async Task PopAsync() => await PopAsync(animated: true)
            .ConfigureAwait(false);

        public async Task PopAsync(bool animated) => await NavigationService.PopAsync(animated);

        public async Task PushModalAsync<TViewModel>(bool animated) where TViewModel : class, IViewModel
        {
            Option<Page> optionalPage = _viewFactory.Resolve<TViewModel>();
            await optionalPage.Match(
                some: async page => await NavigationService.PushModalAsync(page, animated),
                none: () => Task.CompletedTask
            );
        }

        public async Task PushModalAsync<TViewModel>() where TViewModel : class, IViewModel
            => await PushModalAsync<TViewModel>(animated: true);

        public async Task PushModalAsync<TViewModel>(object data)
            where TViewModel : class, IViewModel
            => await PushModalAsync<TViewModel>(data, animated: true);

        public async Task PushModalAsync<TViewModel>(object data, bool animated)
            where TViewModel : class, IViewModel
        {
            Option<Page> optionalPage = _viewFactory.Resolve<TViewModel>(data);

            await optionalPage.Match(
                some: async page => {
                    await NavigationService.PushModalAsync(page, animated)
                     .ConfigureAwait(false);
                },
                none: () => Task.CompletedTask
            );
        }

        /// <summary>
        /// Navigate to the root page
        /// </summary>
        /// <returns></returns>
        public Task PopToRootAsync()
        {
            int pageCount = NavigationService.NavigationStack.Count;

            for (int i = pageCount - 1; i > 0; i--)
            {
                Page page = NavigationService.NavigationStack[i];
                NavigationService.RemovePage(page);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Inserts the page associated with <typeparamref name="TViewModelInserted"/> before the page associated 
        /// with <typeparamref name="TViewModelTarget"/>
        /// </summary>
        /// <typeparam name="TViewModelInserted">Type of page's view model to insert</typeparam>
        /// <typeparam name="TViewModelTarget">Type of the page's view model before which the page associated with
        /// <typeparamref name="TViewModelInserted"/> will be inserted before.
        /// </typeparam>
        public void InsertViewModelBefore<TViewModelInserted, TViewModelTarget>()
            where TViewModelInserted : IViewModel
            where TViewModelTarget : IViewModel
        {

            Option<Page> optionalSource = _viewFactory.Resolve<TViewModelInserted>();
            optionalSource.MatchSome(page =>
            {

                Option<Page> optionalBeforePage = _viewFactory.Resolve<TViewModelTarget>();
                optionalBeforePage.MatchSome(before => NavigationService.InsertPageBefore(page, before));
            });
        }


        public async Task PopModalAsync(bool animated) => await NavigationService.PopModalAsync(animated);

        public async Task PopModalAsync() => await PopModalAsync(animated: true);
        public Task RemoveBackStackAsync()
        {
            int pageToRemoveCount = NavigationService.NavigationStack.Count;

            for (int i = pageToRemoveCount - 2; i >= 0; i--)
            {
                Page pageToRemove = NavigationService.NavigationStack[i];
                NavigationService.RemovePage(pageToRemove);
            }

            return Task.CompletedTask;
        }

        public async Task NavigateTo<TViewModel, TNavigationData>(TNavigationData navigationData, bool animated)
            where TViewModel : class, IViewModel
        {
            Option<Page> optionalPage = _viewFactory.Resolve<TViewModel>(navigationData);

            await optionalPage.Match(
                some: async page => await NavigationService.PushAsync(page, animated)
                        .ConfigureAwait(false),
                none: () => Task.CompletedTask
            )
                .ConfigureAwait(false);
        }

        public async Task NavigateTo<TViewModel, TNavigationData>(TNavigationData navigationData)
            where TViewModel : class, IViewModel
            => await NavigateTo<TViewModel, TNavigationData>(navigationData, animated: true);

        public async Task PushModalAsync<TViewModel, TNavigationData>(TNavigationData navigationData, bool animated)
            where TViewModel : class, IViewModel
        {
            Option<Page> optionalPage = _viewFactory.Resolve<TViewModel>(navigationData);

            await optionalPage.Match(
                some: async page => await NavigationService.PushModalAsync(page, animated)
                        .ConfigureAwait(false),
                none: () => Task.CompletedTask
            )
                .ConfigureAwait(false);
        }

        public async Task PushModalAsync<TViewModel, TNavigationData>(TNavigationData navigationData)
            where TViewModel : class, IViewModel
            => await PushModalAsync<TViewModel, TNavigationData>(navigationData, animated: true)
                .ConfigureAwait(false);
    }
}
