using MedEasy.Mobile.Core.ViewModels.Base;
using System.Threading.Tasks;

namespace MedEasy.Mobile.Core.Services
{
    public interface INavigatorService
    {
        /// <summary>
        /// Navigates to the page with the corresponding view model
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model </typeparam>
        /// <returns></returns>
        Task NavigateTo<TViewModel>()
            where TViewModel : class, IViewModel;

        /// <summary>
        /// Navigate to the page with the corresponding view model
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="animated"></param>
        /// <returns></returns>
        Task NavigateTo<TViewModel>(bool animated)
            where TViewModel : class, IViewModel;

        /// <summary>
        /// Navigate to the page with the corresponding view model
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model to navigate to</typeparam>
        /// <typeparam name="TNavigationData">Type of the navigation data to provide when navigating to the view model</typeparam>
        /// <param name="navigationData">Data to use when rehydrating the view model</param>
        /// <returns></returns>
        Task NavigateTo<TViewModel, TNavigationData>(TNavigationData navigationData)
            where TViewModel : class, IViewModel;

        /// <summary>
        /// Navigate to the page with the corresponding view model
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model to navigate to</typeparam>
        /// <typeparam name="TNavigationData">Type of the navigation data to provide when navigating to the view model</typeparam>
        /// <param name="navigationData">Data to use when rehydrating the view model</param>
        /// <param name="animated">Indicates if an animation should be performed</param>
        /// <returns></returns>
        Task NavigateTo<TViewModel, TNavigationData>(TNavigationData navigationData, bool animated)
            where TViewModel : class, IViewModel;

        /// <summary>
        /// Removes the last inserted element from the navigation stack.
        /// </summary>
        /// <returns></returns>
        Task PopAsync();

        /// <summary>
        /// Adds the page mapped to <typeparamref name="TViewModel"/> to the navigation' stack.
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model</typeparam>
        /// <param name="animated"><c>true</c> to animate the transition</param>
        /// <returns></returns>
        Task PushAsync<TViewModel>(bool animated)
            where TViewModel : class, IViewModel;

        /// <summary>
        /// Removes all but the first element from the navigation' stack
        /// </summary>
        /// 
        /// <returns></returns>
        Task PopToRootAsync();

        /// <summary>
        /// Inserts the page associated with <typeparamref name="TViewModelInserted"/> before the page associated 
        /// with <typeparamref name="TViewModelTarget"/>
        /// </summary>
        /// <typeparam name="TViewModelInserted">Type of page's view model to insert</typeparam>
        /// <typeparam name="TViewModelTarget">Type of the page's view model before which the page associated with
        /// <typeparamref name="TViewModelInserted"/> will be inserted before.
        /// </typeparam>
        void InsertViewModelBefore<TViewModelInserted, TViewModelTarget>()
            where TViewModelInserted : IViewModel
            where TViewModelTarget : IViewModel;

        /// <summary>
        /// Display a view model 
        /// </summary>
        /// <param name="animated"></param>
        /// <typeparam name="TViewModel">Type of the view model to display</typeparam>
        /// <returns></returns>
        Task PushModalAsync<TViewModel>(bool animated) where TViewModel : class, IViewModel;

        /// <summary>
        /// Display a view model in a "modal" fashion
        /// </summary>
        /// <param name="navigationData">Data used to populated the view model</param>
        /// <param name="animated"></param>
        /// <typeparam name="TViewModel">Type of the view model to display</typeparam>
        /// <typeparam name="TNavigationData">Type of the navigation data that will populate the view model</typeparam>
        /// <returns></returns>
        Task PushModalAsync<TViewModel, TNavigationData>(TNavigationData navigationData, bool animated) where TViewModel : class, IViewModel;

        /// <summary>
        /// Display a view model in a "modal" fashion
        /// </summary>
        /// <param name="navigationData">Data used to populated the view model</param>
        /// <typeparam name="TViewModel">Type of the view model to display</typeparam>
        /// <typeparam name="TNavigationData">Type of the navigation data that will populate the view model</typeparam>
        /// <returns></returns>
        Task PushModalAsync<TViewModel, TNavigationData>(TNavigationData navigationData) where TViewModel : class, IViewModel;

        /// <summary>
        /// Display the page associated with a view model in a modal fashion
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model to display</typeparam>
        /// <returns></returns>
        Task PushModalAsync<TViewModel>() where TViewModel : class, IViewModel;

        /// <summary>
        /// Display the page associated with a view model in a modal fashion
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model to display</typeparam>
        /// <returns></returns>

        /// <summary>
        /// Display the page associated with a view model in a modal fashion
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model to display</typeparam>
        /// <param name="data">Data to initialize the view model with</param>
        /// <param name="animated"></param>
        /// <returns></returns>
        Task PushModalAsync<TViewModel>(object navigationData) where TViewModel : class, IViewModel;

        /// <summary>
        /// Display the page associated with a view model in a modal fashion
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model to display</typeparam>
        /// <param name="data">Data to display</param>
        /// <param name="animated"></param>
        /// <returns></returns>
        Task PushModalAsync<TViewModel>(object data, bool animated) where TViewModel : class, IViewModel;

        /// <summary>
        /// Dismiss the modal view currently displayed
        /// </summary>
        /// <returns></returns>
        Task PopModalAsync(bool animated);

        /// <summary>
        /// Dismiss the modal view currently displayed
        /// </summary>
        /// <returns></returns>
        Task PopModalAsync();

        /// <summary>
        /// Remove everything from the navigation stack except the last view
        /// </summary>
        /// <returns></returns>
        Task RemoveBackStackAsync();
    }
}