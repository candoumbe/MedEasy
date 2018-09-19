using FluentAssertions;
using FluentAssertions.Events;
using Identity.DTO;
using MedEasy.Mobile.Core.Apis;
using MedEasy.Mobile.Core.Services;
using MedEasy.Mobile.Core.ViewModels;
using MedEasy.Mobile.Core.ViewModels.Base;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Moq.MockBehavior;

namespace MedEasy.Mobile.UnitTests.Core.ViewModels
{
    [UnitTest]
    [Feature("Mobile")]
    public class SignInViewModelTests : IDisposable
    {
        private readonly ITestOutputHelper _outputHelper;
        private Mock<ITokenApi> _tokenApiMock;
        private Mock<INavigatorService> _navigationServiceMock;
        private SignInViewModel _sut;

        public SignInViewModelTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _tokenApiMock = new Mock<ITokenApi>(Strict);
            _navigationServiceMock = new Mock<INavigatorService>(Strict);
            _sut = new SignInViewModel(_tokenApiMock.Object, _navigationServiceMock.Object);
            
        }

        public void Dispose()
        {
            _tokenApiMock = null;
            _sut = null;

        }

        [Theory]
        [InlineData("", "", false, "username and password are empty")]
        [InlineData("", "smile", false, "password is empty")]
        [InlineData("thejoker", "", false, "password is empty")]
        [InlineData("thejoker", "smile", true, "username and password are not empty")]
        public void SignInCommand_CanExecute_Relies_On_Validator(string username, string password, bool validatorResponse, string reason)
        {
            // Arrange
            IMonitor<SignInViewModel> monitor = _sut.Monitor();
            _sut.Login = username;
            _sut.Password = password;

            
            // Act
            bool canExecute = _sut.SignInCommand.CanExecute(null);

            // Assert
            canExecute.Should()
                .Be(validatorResponse, reason);

        }

        [Fact]
        public async Task SetLogin_Raise_LoginPropertyChangeEvent()
        {
            // Arrange
            IMonitor<SignInViewModel> monitor = _sut.Monitor();
            IMonitor<Command> signInCommandMonitor = _sut.SignInCommand.Monitor();

            // Act
            _sut.Login = "thejoker";

            // Assert
            _outputHelper.WriteLine(monitor.OccurredEvents.Stringify());
            monitor.Should()
                .RaisePropertyChangeFor(vm => vm.Login);
            monitor.Should()
                .NotRaisePropertyChangeFor(vm => vm.Password);
            signInCommandMonitor.Should()
                .Raise(nameof(Command.CanExecuteChanged));
        }

        [Fact]
        public async Task SetPassword_RaisePasswordPropertyChangeEvent()
        {
            // Arrange
            IMonitor<SignInViewModel> vmMonitor = _sut.Monitor();
            IMonitor<Command> signInCommandMonitor = _sut.SignInCommand.Monitor();

            // Act
            _sut.Password = "thesmile";

            // Assert
            vmMonitor.Should()
                .RaisePropertyChangeFor(vm => vm.Password);
            vmMonitor.Should()
                .NotRaisePropertyChangeFor(vm => vm.Login);
            signInCommandMonitor.Should()
                .Raise(nameof(Command.CanExecuteChanged));
            
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("bruce@wayne-entreprise.com")]
        public void LostPasswordCommand_GoTo_SignUpViewModel(string login)
        {
            // Arrange
            _navigationServiceMock.Setup(mock => mock.PushModalAsync(It.IsAny<LostPasswordViewModel>()))
                .Returns(Task.CompletedTask);

            _sut.Login = login;
            // Act
            _sut.LostPasswordCommand.Execute(default);

            // Assert
            _navigationServiceMock.Verify(mock => mock.PushModalAsync(It.IsAny<LostPasswordViewModel>()), Times.Once);
            _navigationServiceMock.Verify(mock => mock.PushModalAsync(It.Is<LostPasswordViewModel>(model => model.Email == login)), Times.Once);

        }

        [Fact]
        public void IsViewModel() => typeof(ViewModelBase).IsAssignableFrom(_sut.GetType())
            .Should().BeTrue();

        [Fact]
        public void SignInCommand_GoTo_HomeViewModel_When_Successfull()
        {
            // Arrange
            _sut.Login = "darkknight";
            _sut.Password = "capedcrusader";

            _tokenApiMock.Setup(mock => mock.SignIn(It.IsAny<LoginInfo>(), It.IsAny<CancellationToken>()))
                .Returns((LoginInfo loginInfo, CancellationToken ct) =>
                {
                    return Task.FromResult(new BearerTokenInfo
                    {
                        AccessToken = $"access-{loginInfo.Username}-{loginInfo.Password}",
                        RefreshToken = $"refresh-{loginInfo.Username}-{loginInfo.Password}"
                    });
                });
            _navigationServiceMock.Setup(mock => mock.InsertViewModelBefore<HomeViewModel, SignInViewModel>());
            _navigationServiceMock.Setup(mock => mock.NavigateTo<HomeViewModel>(It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            // Act
            _sut.SignInCommand.Execute(null);

            // Assert
            _tokenApiMock.Verify(mock => mock.SignIn(It.IsAny<LoginInfo>(), It.IsAny<CancellationToken>()), Times.Once);
            _tokenApiMock.Verify(mock => mock.SignIn(It.Is<LoginInfo>(input => input.Username == _sut.Login && input.Password == _sut.Password), It.IsAny<CancellationToken>()), Times.Once);
            _navigationServiceMock.Verify(mock => mock.InsertViewModelBefore<HomeViewModel, SignInViewModel>(), Times.Once);
            _navigationServiceMock.Verify(mock => mock.NavigateTo<HomeViewModel>(It.IsAny<bool>()), Times.Once);
        }
    }
}
