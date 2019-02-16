using FluentAssertions;
using MedEasy.Mobile.Core.Services;
using MedEasy.Mobile.Core.ViewModels.Base;
using Moq;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using Unity.Lifetime;
using Unity.Registration;
using Unity.Resolution;
using Xamarin.Forms;
using Xunit;
using Xunit.Categories;
using static Moq.MockBehavior;

namespace MedEasy.Mobile.UnitTests.Core.Services
{
    [UnitTest]
    [Feature("Mobile")]
    public class ViewFactoryTests : IDisposable
    {
        public class FirstViewModel : ViewModelBase
        {

            public override void Prepare(object navigationData)
            {
                if (navigationData is FirstViewModel data)
                {
                    IsBusy = data.IsBusy;
                }
            }
        }
        public class SecondViewModel : ViewModelBase { }
        public class ThirdViewModel : ViewModelBase { }


        private Mock<IUnityContainer> _unityContainerMock;
        private ViewFactory _sut;

        public ViewFactoryTests()
        {
            _unityContainerMock = new Mock<IUnityContainer>(Strict);
            _sut = new ViewFactory(_unityContainerMock.Object);
        }

        public void Dispose()
        {
            _unityContainerMock = null;
            _sut = null;
        }


        [Fact]
        public void AddingSameMappingMultipleTimes_Throws_MappingAlreadyPresentException()
        {
            // Arrange
            IUnityContainer container = new UnityContainer();

            _unityContainerMock.Setup(mock => mock.RegisterType(It.IsAny<Type>(), It.IsAny<Type>(), It.IsAny<string>(),
                It.IsAny<LifetimeManager>(), It.IsAny<InjectionMember[]>()))
                .Returns((Type from, Type to, string name, LifetimeManager ltm, InjectionMember[] injectionMembers)
                    => container.RegisterType(from, to, name, ltm, injectionMembers));

            _sut.AddMapping<FirstViewModel, Page>();

            // Act
            Action action = () => _sut.AddMapping<FirstViewModel, Page>();

            // Assert
            action.Should()
                .Throw<ViewModelAlreadyMappedException>();
        }

        [Fact]
        public void GivenNoMapping_Resolve_Returns_None()
        {
            // Arrange


            // Act
            Option<Page> optionalPage = _sut.Resolve<FirstViewModel>();

            // Assert
            optionalPage.HasValue.Should()
                .BeFalse("no mapping for the specified view model");
        }

        [Fact]
        public void GivenMappingExists_Resolve_Returns_Page()
        {
            // Arrange
            IUnityContainer container = new UnityContainer();
            _unityContainerMock.Setup(mock => mock.RegisterType(It.IsAny<Type>(), It.IsAny<Type>(), It.IsAny<string>(),
                It.IsAny<LifetimeManager>(), It.IsAny<InjectionMember[]>()))
                .Returns((Type from, Type to, string name, LifetimeManager ltm, InjectionMember[] injectionMembers)
                    => container.RegisterType(from, to, name, ltm, injectionMembers));

            _unityContainerMock.Setup(mock => mock.Resolve(It.IsAny<Type>(), It.IsAny<string>(), It.IsAny<ResolverOverride[]>()))
                .Returns((Type type, string name, ResolverOverride[] resolvers) => container.Resolve(type, name, resolvers));


            _sut.AddMapping<FirstViewModel, Page>();

            // Act
            Option<Page> optionalPage = _sut.Resolve<FirstViewModel>();

            // Assert
            optionalPage.HasValue.Should()
                .BeTrue("there's mapping for the specified view model");
            optionalPage.MatchSome(
                page => {
                    page.BindingContext.Should()
                        .BeOfType<FirstViewModel>();
                }
            );
        }

        [Fact]
        public void GivenMappingExists_ResolveWith_Returns_PageWithBindingContextSet()
        {
            // Arrange
            IUnityContainer container = new UnityContainer();
            _unityContainerMock.Setup(mock => mock.RegisterType(It.IsAny<Type>(), It.IsAny<Type>(), It.IsAny<string>(),
                It.IsAny<LifetimeManager>(), It.IsAny<InjectionMember[]>()))
                .Returns((Type from, Type to, string name, LifetimeManager ltm, InjectionMember[] injectionMembers)
                    => container.RegisterType(from, to, name, ltm, injectionMembers));

            _unityContainerMock.Setup(mock => mock.Resolve(It.IsAny<Type>(), It.IsAny<string>(), It.IsAny<ResolverOverride[]>()))
                .Returns((Type type, string name, ResolverOverride[] resolvers) => container.Resolve(type, name, resolvers));


            _sut.AddMapping<FirstViewModel, Page>();
            FirstViewModel initializeData = new FirstViewModel
            {
                IsBusy = true
            };

            // Act
            Option<Page> optionalPage = _sut.Resolve<FirstViewModel>(initializeData);

            // Assert
            optionalPage.HasValue.Should()
                .BeTrue("there's mapping for the specified view model");
            optionalPage.MatchSome(page =>
            {
                FirstViewModel bindingContext = page.BindingContext.Should()
                    .NotBeNull().And
                    .BeAssignableTo<FirstViewModel>().Which;

                bindingContext.IsBusy.Should()
                    .Be(initializeData.IsBusy);
            });
        }
    }
}
