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
            where TViewModel : ViewModelBase;

        /// <summary>
        /// Navigate to the page with the corresponding view model
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="animated"></param>
        /// <returns></returns>
        Task NavigateTo<TViewModel>(bool animated)
            where TViewModel : ViewModelBase
            ;
        Task PopAsync();
        Task PushAsync<TViewModel>(bool animated)
            where TViewModel : ViewModelBase;

        Task PopToRootAsync(bool animated);

        /// <summary>
        /// Inserts <typeparamref name="TViewModelInserted"/> before <typeparamref name="TViewModelTarget"/> in the navigation stack
        /// </summary>
        /// <typeparam name="TViewModelInserted">Type of the view model to insert</typeparam>
        /// <typeparam name="TViewModelTarget">Type of the view model where <typeparamref name="TViewModelInserted"/></typeparam>
        void InsertViewModelBefore<TViewModelInserted, TViewModelTarget>()
            where TViewModelInserted : ViewModelBase
            where TViewModelTarget : ViewModelBase;

        /// <summary>
        /// Display a view model 
        /// </summary>
        /// <param name="animated"></param>
        /// <typeparam name="TViewModel">Type of the view model to display</typeparam>
        /// <returns></returns>
        Task PushModalAsync<TViewModel>(bool animated) where TViewModel : ViewModelBase;

        /// <summary>
        /// Display the page associated with a view model in a modal fashion
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model to display</typeparam>
        /// <returns></returns>
        Task PushModalAsync<TViewModel>() where TViewModel : ViewModelBase;

        /// <summary>
        /// Display the page associated with a view model in a modal fashion
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model to display</typeparam>
        /// <param name="data">Data to display</param>
        /// <param name="animated"></param>
        /// <returns></returns>
        Task PushModalAsync<TViewModel>(TViewModel data, bool animated) where TViewModel : ViewModelBase;

        /// <summary>
        /// Display the page associated with a view model in a modal fashion
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model to display</typeparam>
        /// <param name="data">Data to display</param>
        /// <returns></returns>
        Task PushModalAsync<TViewModel>(TViewModel data) where TViewModel : ViewModelBase;


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
    }
}