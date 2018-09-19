using MedEasy.Mobile.Core.ViewModels.Base;
using Optional;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity;
using Xamarin.Forms;

namespace MedEasy.Mobile.Core.Services
{
    /// <summary>
    /// Holds a map between view models and views.
    /// </summary>
    /// <remarks>
    /// They can only be one view model mapping.
    /// </remarks>
    public class ViewFactory : IViewFactory
    {
        private readonly IDictionary<Type, Type> _viewModelToViewMappings;
        private readonly IUnityContainer _container;


        /// <summary>
        /// Builds a new <see cref="ViewFactory"/> instance.
        /// 
        /// </summary>
        public ViewFactory(IUnityContainer container)
        {
            _viewModelToViewMappings = new ConcurrentDictionary<Type, Type>();
            _container = container;
        }


        /// <summary>
        /// Adds a new viewmodel to view mapping
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model</typeparam>
        /// <typeparam name="TView">Type of the view</typeparam>
        /// <exception cref="ViewModelAlreadyMappedException">if <typeparamref name="TViewModel"/> was already mapped.</exception>
        public void AddMapping<TViewModel, TView>()
            where TView : Page
            where TViewModel : ViewModelBase
        {
            if (_viewModelToViewMappings.ContainsKey(typeof(TViewModel)))
            {
                throw new ViewModelAlreadyMappedException(typeof(TViewModel));
            }
            _viewModelToViewMappings.Add(typeof(TViewModel), typeof(TView));
            _container.RegisterType<TViewModel>();
            _container.RegisterType<TView>();
        }

        /// <summary>
        /// Finds the corresponding page mapped to <typeparamref name="TViewModel"/>
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model</typeparam>
        /// <returns></returns>
        public Option<Page> Resolve<TViewModel>()
            where TViewModel : ViewModelBase

        {
            Option<Page> optionalPage = Option.None<Page>();
            if (_viewModelToViewMappings.TryGetValue(typeof(TViewModel), out Type pageType))
            {
                if (_container.Resolve(pageType) is Page page)
                {
                    page.BindingContext = _container.Resolve<TViewModel>();
                    optionalPage = page.Some();
                }
            }

            return optionalPage;
        }

        /// <summary>
        /// Finds the corresponding page mapped to <typeparamref name="TViewModel"/>
        /// </summary>
        /// <typeparam name="TViewModel">Type of the view model</typeparam>
        /// <returns></returns>
        /// <remarks>
        /// The page, if any, is rehydrated with <paramref name="data"/>
        /// </remarks>
        public Option<Page> Resolve<TViewModel>(TViewModel data)
            where TViewModel : ViewModelBase
        {
            Option<Page> optionalPage = Option.None<Page>();
            if (_viewModelToViewMappings.TryGetValue(typeof(TViewModel), out Type typePage))
            {
                if (_container.Resolve(typePage) is Page page)
                {
                    page.BindingContext = data;
                    optionalPage = page.Some();
                }
            }

            return optionalPage;
        }
    }
}
