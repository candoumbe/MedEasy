using MedEasy.Mobile.Core.Services;
using MedEasy.Mobile.Core.ViewModels.Base;
using Moq;
using Optional;
using System;
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
    public class NavigatorServiceTests : IDisposable
    {
        private Mock<IViewFactory> _viewFactoryMock;
        private Mock<INavigation> _navigationServiceMock;
        private NavigatorService _sut;

        public class FirstViewModel : ViewModelBase { }
        public class SecondViewModel : ViewModelBase { }
        public class ThirdViewModel : ViewModelBase { }

        public NavigatorServiceTests()
        {
            _viewFactoryMock = new Mock<IViewFactory>(Strict);
            _navigationServiceMock = new Mock<INavigation>(Strict);

            _sut = new NavigatorService(_viewFactoryMock.Object, new Lazy<INavigation>(() => _navigationServiceMock.Object));
        }

        public void Dispose()
        {
            _viewFactoryMock = null;
            _navigationServiceMock = null;
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

            _viewFactoryMock.Setup(mock => mock.Resolve(It.IsAny<ThirdViewModel>()))
                .Returns((ThirdViewModel vm) =>
                {
                    thirdPage.BindingContext = vm;
                    return thirdPage.Some();
                });
            _viewFactoryMock.Setup(mock => mock.Resolve<SecondViewModel>())
                .Returns(secondPage.Some());

            _navigationServiceMock.Setup(mock => mock.PushModalAsync(It.IsAny<Page>()))
                 .Returns(Task.CompletedTask);

            // Act
            await _sut.PushModalAsync(thirdVm, animated)
                .ConfigureAwait(false);

            // Assert
            _viewFactoryMock.Verify(mock => mock.Resolve(It.IsAny<ThirdViewModel>()), Times.Once);
            _navigationServiceMock.Verify(mock => mock.PushModalAsync(It.IsAny<Page>()), Times.Once, "view model is mapped to a page");
            _navigationServiceMock.Verify(mock => mock.PushModalAsync(It.Is<Page>(page => thirdPage == page && thirdPage.BindingContext == thirdVm)), Times.Once, "view model exists and is mapped to a page");
        }

        [Fact]
        public async Task PopModalAsync()
        {
            // Arrange
            _navigationServiceMock.Setup(mock => mock.PopModalAsync(It.IsAny<bool>()))
                .ReturnsAsync(Mock.Of<Page>());

            // Act
            await _sut.PopModalAsync();

            // Assert
            _navigationServiceMock.Verify(mock => mock.PopModalAsync(It.IsAny<bool>()), Times.Once);
        }
    }
}
