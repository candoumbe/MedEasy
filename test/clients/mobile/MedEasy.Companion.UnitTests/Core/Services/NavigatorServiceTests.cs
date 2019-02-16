using FluentAssertions;
using MedEasy.Mobile.Core.Apis;
using MedEasy.Mobile.Core.Services;
using MedEasy.Mobile.Core.ViewModels;
using MedEasy.Mobile.Core.ViewModels.Base;
using Moq;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xunit;
using Xunit.Categories;
using static Moq.MockBehavior;

namespace MedEasy.Mobile.UnitTests.Core.Services
{
    [UnitTest]
    [Feature("Mobile")]
    [Feature("Navigator")]
    public class NavigationServiceTests : IDisposable
    {
        private Mock<IViewFactory> _viewFactoryMock;
        private Mock<INavigation> _navigationServiceMock;
        private Mock<IApplicationService> _applicationServiceMock;
        private NavigatorService _sut;

        public class FirstViewModel : ViewModelBase { }
        public class SecondViewModel : ViewModelBase { }
        public class ThirdViewModel : ViewModelBase
        {

            public override void Prepare(object navigationData)
            {
                if (navigationData is ThirdViewModel model)
                {
                    IsBusy = model.IsBusy;
                }
            }
        }

        public NavigationServiceTests()
        {
            _viewFactoryMock = new Mock<IViewFactory>(Strict);
            _navigationServiceMock = new Mock<INavigation>(Strict);
            _applicationServiceMock = new Mock<IApplicationService>(Strict);
            _sut = new NavigatorService(_viewFactoryMock.Object, new Lazy<INavigation>(() => _navigationServiceMock.Object), _applicationServiceMock.Object);
        }

        public void Dispose()
        {
            _viewFactoryMock = null;
            _navigationServiceMock = null;
            _applicationServiceMock = null;
            _sut = null;
        }


        [Fact]
        public async Task GivenEmptyViewFactory_Push_Does_Nothing()
        {
            // Arrange
            _viewFactoryMock.Setup(mock => mock.Resolve<FirstViewModel>())
                .Returns(Option.None<Page>());

            // Act
            await _sut.PushAsync<FirstViewModel>(animated: true)
                .ConfigureAwait(false);

            // Assert
            _viewFactoryMock.Verify(mock => mock.Resolve<FirstViewModel>(), Times.Once);
            _navigationServiceMock.Verify(mock => mock.PushAsync(It.IsAny<Page>(), It.IsAny<bool>()), Times.Never, "The viewmodel -> view mapping does not exist");
            _navigationServiceMock.Verify(mock => mock.PopAsync(It.IsAny<bool>()), Times.Never, "It's a push");
            _navigationServiceMock.Verify(mock => mock.PopToRootAsync(It.IsAny<bool>()), Times.Never, "It's a push");
            _navigationServiceMock.Verify(mock => mock.PopModalAsync(It.IsAny<bool>()), Times.Never, "It's a push");
            _navigationServiceMock.Verify(mock => mock.RemovePage(It.IsAny<Page>()), Times.Never, "It's a push");

        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Push(bool animated)
        {
            // Arrange
            FirstViewModel firstViewModel = Mock.Of<FirstViewModel>();
            Page firstPage = Mock.Of<Page>();
            SecondViewModel second = Mock.Of<SecondViewModel>();
            Page secondPage = Mock.Of<Page>();
            ThirdViewModel third = Mock.Of<ThirdViewModel>();
            Page thirdPage = Mock.Of<Page>();

            _viewFactoryMock.Setup(mock => mock.Resolve<FirstViewModel>())
                .Returns(firstPage.Some());

            _viewFactoryMock.Setup(mock => mock.Resolve<SecondViewModel>())
                .Returns(secondPage.Some());
            _navigationServiceMock.Setup(mock => mock.PushAsync(It.IsAny<Page>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.PushAsync<FirstViewModel>(animated)
                .ConfigureAwait(false);

            // Assert
            _viewFactoryMock.Verify(mock => mock.Resolve<FirstViewModel>(), Times.Once);
            _navigationServiceMock.Verify(mock => mock.PushAsync(It.IsAny<Page>(), It.IsAny<bool>()), Times.Once, "The viewmodel -> view mapping exists");
            _navigationServiceMock.Verify(mock => mock.PushAsync(It.Is<Page>(p => p == firstPage), It.Is<bool>(actualAnimated => actualAnimated == animated)), Times.Once, "The viewmodel -> view mapping exists");
            _navigationServiceMock.Verify(mock => mock.PopAsync(It.IsAny<bool>()), Times.Never, "It's a push");
            _navigationServiceMock.Verify(mock => mock.PopToRootAsync(It.IsAny<bool>()), Times.Never, "It's a push");
            _navigationServiceMock.Verify(mock => mock.PopModalAsync(It.IsAny<bool>()), Times.Never, "It's a push");
            _navigationServiceMock.Verify(mock => mock.RemovePage(It.IsAny<Page>()), Times.Never, "It's a push");

        }


        [Fact]
        public void InsertBefore_Is_NoOp_When_Target_Dont_Exist()
        {
            // Arrange
            FirstViewModel firstViewModel = Mock.Of<FirstViewModel>();
            Page firstPage = Mock.Of<Page>();
            SecondViewModel second = Mock.Of<SecondViewModel>();
            Page secondPage = Mock.Of<Page>();
            ThirdViewModel third = Mock.Of<ThirdViewModel>();
            Page thirdPage = Mock.Of<Page>();

            _viewFactoryMock.Setup(mock => mock.Resolve<ThirdViewModel>())
                .Returns(thirdPage.Some());

            _viewFactoryMock.Setup(mock => mock.Resolve<SecondViewModel>())
                .Returns(Option.None<Page>());

            // Act
            _sut.InsertViewModelBefore<ThirdViewModel, SecondViewModel>();

            // Assert
            _viewFactoryMock.Verify(mock => mock.Resolve<ThirdViewModel>(), Times.Once);
            _viewFactoryMock.Verify(mock => mock.Resolve<SecondViewModel>(), Times.Once);
            _navigationServiceMock.Verify(mock => mock.InsertPageBefore(It.IsAny<Page>(), It.IsAny<Page>()), Times.Never, "The target viewmodel does not exist in the mapping");

            _applicationServiceMock.VerifyNoOtherCalls();
            _navigationServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task NavigateToSignInViewModel_Set_MainPage_ToSignInPage()
        {
            // Arrange
            SignInViewModel signInViewModel = new SignInViewModel(Mock.Of<INavigatorService>(), Mock.Of<ITokenApi>());
            Page signInPage = new NavigationPage(Mock.Of<Page>());

            _viewFactoryMock.Setup(mock => mock.Resolve<SignInViewModel>())
                .Returns(signInPage.Some());

            _applicationServiceMock.SetupProperty(mock => mock.MainPage);

            // Act
            await _sut.NavigateTo<SignInViewModel>()
                .ConfigureAwait(false);

            // Assert
            _navigationServiceMock.Verify(mock => mock.PushAsync(It.IsAny<Page>(), It.IsAny<bool>()), Times.Never);
            _applicationServiceMock.VerifySet(mock => mock.MainPage = It.Is<Page>(page => page is NavigationPage
                && ((NavigationPage)page).RootPage == signInPage), Times.Once);

            _applicationServiceMock.VerifyNoOtherCalls();
            _navigationServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void InsertBefore_Is_NoOp_When_ViewModelToInsert_Dont_Exist()
        {
            // Arrange
            FirstViewModel firstViewModel = Mock.Of<FirstViewModel>();
            Page firstPage = Mock.Of<Page>();
            SecondViewModel second = Mock.Of<SecondViewModel>();
            Page secondPage = Mock.Of<Page>();
            ThirdViewModel third = Mock.Of<ThirdViewModel>();
            Page thirdPage = Mock.Of<Page>();

            _viewFactoryMock.Setup(mock => mock.Resolve<ThirdViewModel>())
                .Returns(Option.None<Page>());

            // Act
            _sut.InsertViewModelBefore<ThirdViewModel, SecondViewModel>();

            // Assert
            _viewFactoryMock.Verify(mock => mock.Resolve<ThirdViewModel>(), Times.Once);
            _viewFactoryMock.Verify(mock => mock.Resolve<SecondViewModel>(), Times.Never, "No mapping for the source viewmodel");
            _navigationServiceMock.Verify(mock => mock.InsertPageBefore(It.IsAny<Page>(), It.IsAny<Page>()), Times.Never, "The target viewmodel does not exist in the mapping");

            _applicationServiceMock.VerifyNoOtherCalls();
            _navigationServiceMock.VerifyNoOtherCalls();
        }



        [Fact]
        public void InsertBefore_Is_Done_When_ViewModelSourceAndTarget_Exist()
        {
            // Arrange
            FirstViewModel firstViewModel = Mock.Of<FirstViewModel>();
            Page firstPage = Mock.Of<Page>();
            SecondViewModel second = Mock.Of<SecondViewModel>();
            Page secondPage = Mock.Of<Page>();
            ThirdViewModel third = Mock.Of<ThirdViewModel>();
            Page thirdPage = Mock.Of<Page>();

            _viewFactoryMock.Setup(mock => mock.Resolve<ThirdViewModel>())
                .Returns(thirdPage.Some());
            _viewFactoryMock.Setup(mock => mock.Resolve<SecondViewModel>())
                .Returns(secondPage.Some());

            _navigationServiceMock.Setup(mock => mock.InsertPageBefore(It.IsAny<Page>(), It.IsAny<Page>()));

            // Act
            _sut.InsertViewModelBefore<ThirdViewModel, SecondViewModel>();

            // Assert
            _viewFactoryMock.Verify(mock => mock.Resolve<ThirdViewModel>(), Times.Once);
            _viewFactoryMock.Verify(mock => mock.Resolve<SecondViewModel>(), Times.Once, "Mapping for the source and target view models");
            _navigationServiceMock.Verify(mock => mock.InsertPageBefore(It.IsAny<Page>(), It.IsAny<Page>()), Times.Once, "Both view models have mappings");
            _navigationServiceMock.Verify(mock => mock.InsertPageBefore(It.Is<Page>(page => thirdPage == page), It.Is<Page>(page => secondPage == page)), Times.Once, "Both view models have mappings");

            _applicationServiceMock.VerifyNoOtherCalls();
            _navigationServiceMock.VerifyNoOtherCalls();
        }

        public static IEnumerable<object[]> RemoveBackStackCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<Page>(),
                    (Expression<Func<IReadOnlyList<Page>, bool>>)(stack => stack.Count == 0),
                    "The navigation stack is empty"
                };
                {
                    Page firstPage = Mock.Of<Page>();
                    Page middlePage = Mock.Of<Page>();
                    Page lastPage = Mock.Of<Page>();

                    yield return new object[]
                    {
                        new []{ firstPage, middlePage, lastPage },
                        (Expression<Func<IReadOnlyList<Page>, bool>>)(stack => stack.Once() && stack.Once(page => page == lastPage)),
                        "The navigation stack contains 3 elements"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(RemoveBackStackCases))]
        public async Task RemoveBackStackAsync(IEnumerable<Page> currentStack, Expression<Func<IReadOnlyList<Page>, bool>> navigationStackExpectation, string reason)
        {
            // Arrange
            List<Page> navigationStack = new List<Page>(currentStack);

            _navigationServiceMock.Setup(mock => mock.NavigationStack.Count)
                .Returns(currentStack.Count);
            _navigationServiceMock.Setup(mock => mock.NavigationStack)
                .Returns(navigationStack.AsReadOnly());

            _navigationServiceMock.Setup(mock => mock.RemovePage(It.IsAny<Page>()))
                .Callback((Page pageToRemove) => navigationStack.Remove(pageToRemove));

            // Act
            await _sut.RemoveBackStackAsync()
                .ConfigureAwait(false);

            // Assert
            navigationStack.Should()
                .Match(navigationStackExpectation, reason);
            //_navigationServiceMock.Verify(mock => mock.NavigationStack.Count, Times.Once);
            //_navigationServiceMock.Verify(mock => mock.NavigationStack[It.IsAny<int>()], Times.Exactly(Math.Max(0, currentStack.Count() - 1)));
            _navigationServiceMock.Verify(mock => mock.RemovePage(It.IsAny<Page>()), Times.Exactly(Math.Max(0, currentStack.Count() - 1)));

        }


        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task PushModalAsync(bool animated)
        {
            // Arrange
            FirstViewModel firstViewModel = Mock.Of<FirstViewModel>();
            Page firstPage = Mock.Of<Page>();
            SecondViewModel second = Mock.Of<SecondViewModel>();
            Page secondPage = Mock.Of<Page>();
            ThirdViewModel third = Mock.Of<ThirdViewModel>();
            Page thirdPage = Mock.Of<Page>();


            _viewFactoryMock.Setup(mock => mock.Resolve<ThirdViewModel>())
                .Returns(thirdPage.Some());
            _viewFactoryMock.Setup(mock => mock.Resolve<SecondViewModel>())
                .Returns(secondPage.Some());

            _navigationServiceMock.Setup(mock => mock.PushModalAsync(It.IsAny<Page>(), It.IsAny<bool>()))
                 .Returns(Task.CompletedTask);

            // Act
            await _sut.PushModalAsync<ThirdViewModel>(animated)
                .ConfigureAwait(false);


            // Assert
            _viewFactoryMock.Verify(mock => mock.Resolve<ThirdViewModel>(), Times.Once);
            _navigationServiceMock.Verify(mock => mock.PushModalAsync(It.IsAny<Page>(), It.IsAny<bool>()), Times.Once, "view model is mapped to a page");
            _navigationServiceMock.Verify(mock => mock.PushModalAsync(It.Is<Page>(page => thirdPage == page), It.Is<bool>(b => animated == b)), Times.Once, "view model exists and is mapped to a page");

            _applicationServiceMock.VerifyNoOtherCalls();
            _viewFactoryMock.VerifyNoOtherCalls();
            _navigationServiceMock.VerifyNoOtherCalls();
        }


        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task PushModalAsyncWithData(bool animated)
        {
            // Arrange
            FirstViewModel firstViewModel = Mock.Of<FirstViewModel>();
            Page firstPage = Mock.Of<Page>();
            SecondViewModel second = Mock.Of<SecondViewModel>();
            Page secondPage = Mock.Of<Page>();
            ThirdViewModel thirdVm = Mock.Of<ThirdViewModel>();
            Page thirdPage = Mock.Of<Page>();

            _viewFactoryMock.Setup(mock => mock.Resolve<ThirdViewModel>(It.IsAny<object>()))
                .Returns((object initialData) =>
                {
                    IViewModel model = new ThirdViewModel();
                    model.Prepare();
                    model.Prepare(initialData);

                    thirdPage.BindingContext = model;
                    return thirdPage.Some();
                });
            _viewFactoryMock.Setup(mock => mock.Resolve<SecondViewModel>())
                .Returns(secondPage.Some());

            _navigationServiceMock.Setup(mock => mock.PushModalAsync(It.IsAny<Page>(), It.IsAny<bool>()))
                 .Returns(Task.CompletedTask);

            // Act
            ThirdViewModel initDataVm = new ThirdViewModel { IsBusy = true };
            await _sut.PushModalAsync<ThirdViewModel>(initDataVm, animated)
                .ConfigureAwait(false);

            // Assert
            _viewFactoryMock.Verify(mock => mock.Resolve<ThirdViewModel>(It.IsAny<ThirdViewModel>()), Times.Once);
            _navigationServiceMock.Verify(mock => mock.PushModalAsync(It.IsAny<Page>(), It.IsAny<bool>()), Times.Once, "view model is mapped to a page");
            _navigationServiceMock.Verify(mock => mock.PushModalAsync(It.Is<Page>(page => thirdPage == page
                && ((ThirdViewModel)thirdPage.BindingContext).IsBusy == initDataVm.IsBusy), It.Is<bool>(param => param == animated)), Times.Once, "view model exists and is mapped to a page");

            _viewFactoryMock.VerifyNoOtherCalls();
            _navigationServiceMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task PopModalAsync(bool animated)
        {
            // Arrange
            _navigationServiceMock.Setup(mock => mock.PopModalAsync(It.IsAny<bool>()))
                .ReturnsAsync(Mock.Of<Page>());

            // Act
            await _sut.PopModalAsync(animated)
                .ConfigureAwait(false);

            // Assert
            _navigationServiceMock.Verify(mock => mock.PopModalAsync(It.IsAny<bool>()), Times.Once);
            _navigationServiceMock.Verify(mock => mock.PopModalAsync(It.Is<bool>(currentAnimated => animated == currentAnimated)), Times.Once);

            _navigationServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task PopToRootAsync()
        {
            // Arrange
            FirstViewModel firstViewModel = Mock.Of<FirstViewModel>();
            Page firstPage = Mock.Of<Page>();
            SecondViewModel second = Mock.Of<SecondViewModel>();
            Page secondPage = Mock.Of<Page>();
            ThirdViewModel thirdVm = Mock.Of<ThirdViewModel>();
            Page thirdPage = Mock.Of<Page>();

            List<Page> pages = new List<Page> { firstPage, secondPage, thirdPage };
            _navigationServiceMock.Setup(mock => mock.NavigationStack)
                .Returns(pages.AsReadOnly());

            _navigationServiceMock.Setup(mock => mock.RemovePage(It.IsAny<Page>()))
                .Callback((Page page) => pages.Remove(page));
            _navigationServiceMock.Setup(mock => mock.NavigationStack[It.IsAny<int>()])
                .Returns((int index) => pages[index]);
            _navigationServiceMock.Setup(mock => mock.NavigationStack.Count)
                .Returns(pages.Count);

            // Act
            await _sut.PopToRootAsync()
                .ConfigureAwait(false);

            // Assert
            _navigationServiceMock.Verify(mock => mock.NavigationStack.Count, Times.Once);
            _navigationServiceMock.Verify(mock => mock.RemovePage(It.IsAny<Page>()), Times.Exactly(2));
            _navigationServiceMock.Verify(mock => mock.RemovePage(It.Is<Page>(page => page == thirdPage)), Times.Once);
            _navigationServiceMock.Verify(mock => mock.RemovePage(It.Is<Page>(page => page == secondPage)), Times.Once);

            _navigationServiceMock.Verify(mock => mock.NavigationStack[It.IsAny<int>()], Times.Exactly(2));
            _navigationServiceMock.VerifyNoOtherCalls();

            pages.Should()
                .HaveCount(1).And
                .ContainSingle(page => page == firstPage);

        }
    }
}
