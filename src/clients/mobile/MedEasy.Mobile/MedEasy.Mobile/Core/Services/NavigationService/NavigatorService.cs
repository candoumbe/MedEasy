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

        /// <summary>
        /// Builds a new <see cref="NavigatorService"/> instance.
        /// </summary>
        /// <param name="viewFactory"></param>
        /// <param name="navigationService"></param>
        public NavigatorService(IViewFactory viewFactory, Lazy<INavigation> navigationService)
        {
            _viewFactory = viewFactory;
            _navigationService = navigationService;
        }

        private INavigation NavigationService => _navigationService.Value;

        public async Task NavigateTo<TViewModel>() where TViewModel : ViewModelBase
            => await NavigateTo<TViewModel>(animated: true)
            .ConfigureAwait(false);

        public async Task NavigateTo<TViewModel>(bool animated)
            where TViewModel : ViewModelBase => await PushAsync<TViewModel>(animated)
            .ConfigureAwait(false);

        public async Task PushAsync<TViewModel>(bool animated)
            where TViewModel : ViewModelBase
        {
            Option<Page> optionalPage = _viewFactory.Resolve<TViewModel>();
            await optionalPage.Match(
                some: async page => await NavigationService.PushAsync(page, animated),
                none: () => Task.CompletedTask
            )
            .ConfigureAwait(false);
        }

        public async Task PopAsync() => await PopAsync(animated: true)
            .ConfigureAwait(false);

        public async Task PopAsync(bool animated) => await NavigationService.PopAsync(animated);

        public async Task PushModalAsync<TViewModel>(bool animated) where TViewModel : ViewModelBase
        {
            Option<Page> optionalPage = _viewFactory.Resolve<TViewModel>();
            await optionalPage.Match(
                some: async page => await NavigationService.PushModalAsync(page, animated),
                none: () => Task.CompletedTask
            );
        }

        public async Task PushModalAsync<TViewModel>() where TViewModel : ViewModelBase
            => await PushModalAsync<TViewModel>(animated : true);

        public async Task PushModalAsync<TViewModel>(TViewModel model) where TViewModel : ViewModelBase
            => await PushModalAsync(model, animated: true);

        public async Task PushModalAsync<TViewModel>(TViewModel model, bool animated) where TViewModel : ViewModelBase
        {
            Option<Page> optionalPage = _viewFactory.Resolve(model);

            await optionalPage.Match(
                some: async page => await NavigationService.PushModalAsync(page),
                none: () => Task.CompletedTask
            );
        }

        public Task PopToRootAsync(bool animated) => NavigationService.PopToRootAsync(animated);
        public void InsertViewModelBefore<TViewModelInserted, TViewModelTarget>()
            where TViewModelInserted : ViewModelBase
            where TViewModelTarget : ViewModelBase
        {
            Option<Page> optionalSource = _viewFactory.Resolve<TViewModelInserted>();
            optionalSource.MatchSome(target =>
            {
                Option<Page> optionalDest = _viewFactory.Resolve<TViewModelTarget>();
                optionalDest.MatchSome(dest => NavigationService.InsertPageBefore(target, dest));
            });
        }


        public async Task PopModalAsync(bool animated) => await NavigationService.PopModalAsync(animated);

        public async Task PopModalAsync() => await PopModalAsync(animated: true);
    }
}
