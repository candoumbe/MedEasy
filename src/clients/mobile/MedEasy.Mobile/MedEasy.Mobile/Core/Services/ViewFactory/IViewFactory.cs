using MedEasy.Mobile.Core.ViewModels.Base;
using Optional;
using Xamarin.Forms;

namespace MedEasy.Mobile.Core.Services
{
    public interface IViewFactory
    {
        /// <summary>
        /// Adds a new <typeparamref name="TViewModel"/> -> <typeparamref name="TView"/> mapping
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model</typeparam>
        /// <typeparam name="TView">Type of the view</typeparam>
        /// <remarks>
        /// </remarks>
        void AddMapping<TViewModel, TView>()
            where TViewModel : ViewModelBase
            where TView : Page;

        /// <summary>
        /// Looks for a <see cref="Page"/>
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model of the association</typeparam>
        /// <returns></returns>
        Option<Page> Resolve<TViewModel>()
             where TViewModel : ViewModelBase;

        /// <summary>
        /// Finds the corresponding page mapped to <typeparamref name="TViewModel"/>
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model</typeparam>
        /// <returns></returns>
        /// <remarks>
        /// The page, if any, is rehydrated with <paramref name="data"/>
        /// </remarks>
        Option<Page> Resolve<TViewModel>(TViewModel data)
             where TViewModel : ViewModelBase;
    }
}