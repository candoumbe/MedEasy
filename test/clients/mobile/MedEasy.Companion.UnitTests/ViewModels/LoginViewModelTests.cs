using FluentAssertions;
using FluentAssertions.Events;
using FluentValidation;
using FluentValidation.Results;
using Identity.DTO;
using MedEasy.Companion.Core.Apis;
using MedEasy.Companion.ViewModels;
using Moq;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Moq.MockBehavior;

namespace MedEasy.Companion.UnitTests
{
    [UnitTest]
    [Feature("Mobile Application")]
    public class LoginViewModelTests : IDisposable
    {
        private readonly ITestOutputHelper _outputHelper;
        private Mock<ITokenApi> _tokenApiMock;
        private Mock<IMvxNavigationService> _navigationServiceMock;
        private Mock<IValidator<LoginInfo>> _loginValidatorMock;
        private LoginViewModel _sut;

        public LoginViewModelTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _tokenApiMock = new Mock<ITokenApi>(Strict);
            _navigationServiceMock = new Mock<IMvxNavigationService>(Strict);
            _loginValidatorMock = new Mock<IValidator<LoginInfo>>(Strict);
            _sut = new LoginViewModel(_tokenApiMock.Object, _loginValidatorMock.Object, _navigationServiceMock.Object);
            _sut.ShouldAlwaysRaiseInpcOnUserInterfaceThread(false); // allows propertychange events to be monitored (https://www.iambacon.co.uk/blog/testing-property-changed-events-with-mvvmcross)

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
        public async Task SignInCommand_CanExecute_Relies_On_Validator(string username, string password, bool validatorResponse, string reason)
        {
            // Arrange
            await _sut.Initialize()
                .ConfigureAwait(false);
            _sut.Login = username;
            _sut.Password = password;

            _loginValidatorMock.Setup(mock => mock.Validate(It.IsAny<LoginInfo>()))
                .Returns((LoginInfo loginInfo) =>
                {
                    ValidationResult vr = new ValidationResult();

                    if (!validatorResponse)
                    {
                        vr.Errors.Add(new ValidationFailure(nameof(LoginInfo.Username), reason));
                    }

                    return vr;
                });

            // Act
            bool canExecute = _sut.SignInCommand.CanExecute(null);

            // Assert
            _loginValidatorMock.Verify(mock => mock.Validate(It.IsAny<LoginInfo>()), Times.Once);
            canExecute.Should()
                .Be(validatorResponse, reason);

        }

        [Fact]
        public async Task SetLogin_Raise_LoginPropertyChangeEvent()
        {
            // Arrange
            await _sut.Initialize()
                .ConfigureAwait(false);
            IMonitor<LoginViewModel> monitor = _sut.Monitor();

            // Act
            _sut.Login = "thejoker";

            // Assert
            _outputHelper.WriteLine(monitor.OccurredEvents.Stringify());
            monitor.Should()
                .RaisePropertyChangeFor(vm => vm.Login);
            monitor.Should()
                .NotRaisePropertyChangeFor(vm => vm.Password);
        }

        [Fact]
        public async Task SetPassword_RaisePasswordPropertyChangeEvent()
        {
            // Arrange
            await _sut.Initialize()
                .ConfigureAwait(false);
            IMonitor<LoginViewModel> monitor = _sut.Monitor();

            // Act
            _sut.Password = "thesmile";

            // Assert
            monitor.Should()
                .RaisePropertyChangeFor(vm => vm.Password);
            monitor.Should()
                .NotRaisePropertyChangeFor(vm => vm.Login);
        }

        [Fact]
        public void SignUpCommand_GoTo_SignUpViewModel()
        {
            // Arrange
            _navigationServiceMock.Setup(mock => mock.Navigate<SignUpViewModel>(It.IsAny<IMvxBundle>()))
                .Returns(Task.CompletedTask);

            // Act
            _sut.SignUpCommand.Execute();

            // Assert
            _navigationServiceMock.Verify(mock => mock.Navigate<SignUpViewModel>(It.IsAny<IMvxBundle>()), Times.Once);

        }
    }
}
