using Bogus;
using FluentAssertions;
using FluentAssertions.Events;
using FluentValidation;
using Identity.DTO;
using MedEasy.Companion.Core.Apis;
using MedEasy.Companion.ViewModels;
using Moq;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Moq.MockBehavior;

namespace MedEasy.Companion.UnitTests
{
    [UnitTest]
    [Feature("Mobile Application")]
    public class SignUpViewModelTests : IDisposable
    {
        private readonly ITestOutputHelper _outputHelper;
        private Mock<IAccountsApi> _accountsApiMock;
        private Mock<IMvxNavigationService> _navigationServiceMock;
        private Mock<IValidator<NewAccountInfo>> _newAccountInfoValidatorMock;
        private SignUpViewModel _sut;

        public SignUpViewModelTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _accountsApiMock = new Mock<IAccountsApi>(Strict);
            _navigationServiceMock = new Mock<IMvxNavigationService>(Strict);
            _newAccountInfoValidatorMock = new Mock<IValidator<NewAccountInfo>>(Strict);
            _sut = new SignUpViewModel(_accountsApiMock.Object, _newAccountInfoValidatorMock.Object, _navigationServiceMock.Object);
            _sut.ShouldAlwaysRaiseInpcOnUserInterfaceThread(false);

        }

        public void Dispose()
        {
            _accountsApiMock = null;
            _navigationServiceMock = null;
            _newAccountInfoValidatorMock = null;
            _sut = null;

        }

        [Theory]
        [InlineData(false, "Validator returns false")]
        [InlineData(true, "Validator returns true")]
        public async Task SignUpCommand_CanExecute_Relies_On_Validator(bool validatorResponse, string reason)
        {
            // Arrange
            Faker faker = new Faker();

            await _sut.Initialize()
                .ConfigureAwait(false);


            _newAccountInfoValidatorMock.Setup(mock => mock.Validate(It.IsAny<NewAccountInfo>()).IsValid)
                .Returns(validatorResponse);

            // Act
            bool canExecute = _sut.SignUpCommand.CanExecute(null);

            // Assert
            _newAccountInfoValidatorMock.Verify(mock => mock.Validate(It.IsAny<NewAccountInfo>()), Times.Once);
            canExecute.Should()
                .Be(validatorResponse, reason);

        }

        [Fact]
        public async Task SignInCommand_CanExecute_ShouldBeTrue() => _sut.SignInCommand.CanExecute(null)
            .Should().BeTrue();


        [Fact]
        public async Task SignInCommand_NavigatesTo_LoginView()
        {
            // Arrange
            _navigationServiceMock.Setup(mock => mock.Navigate<LoginViewModel>(It.IsAny<IMvxBundle>()))
                .Returns(Task.CompletedTask);

            // Act
            _sut.SignInCommand.Execute();

            //Assert
            _navigationServiceMock.Verify(mock => mock.Navigate<LoginViewModel>(It.IsAny<IMvxBundle>()));
        }

        [Fact]
        public async Task SetName_Raise_LoginPropertyChangeEvent()
        {
            // Arrange
            await _sut.Initialize()
                .ConfigureAwait(false);
            IMonitor<SignUpViewModel> monitor = _sut.Monitor();

            // Act
            _sut.Name = "Joker";

            // Assert

            monitor.Should().RaisePropertyChangeFor(vm => vm.Name);
            monitor.Should().NotRaisePropertyChangeFor(vm => vm.ConfirmPassword);
            monitor.Should().NotRaisePropertyChangeFor(vm => vm.Password);
            monitor.Should().NotRaisePropertyChangeFor(vm => vm.Email);
            monitor.Should().NotRaisePropertyChangeFor(vm => vm.Username);
        }

        [Fact]
        public async Task SetUsername_RaiseUsernamePropertyChangeEvent()
        {
            // Arrange
            await _sut.Initialize()
                .ConfigureAwait(false);
            IMonitor<SignUpViewModel> monitor = _sut.Monitor();

            // Act
            _sut.Username = "thesmile";

            // Assert
            monitor.Should().RaisePropertyChangeFor(vm => vm.Username);
            monitor.Should().NotRaisePropertyChangeFor(vm => vm.ConfirmPassword);
            monitor.Should().NotRaisePropertyChangeFor(vm => vm.Password);
            monitor.Should().NotRaisePropertyChangeFor(vm => vm.Email);
            monitor.Should().NotRaisePropertyChangeFor(vm => vm.Name);
        }

        [Fact]
        public async Task SetConfirmPassword_RaiseConfirmPasswordPropertyChangeEvent()
        {
            // Arrange
            await _sut.Initialize()
                .ConfigureAwait(false);
            IMonitor<SignUpViewModel> monitor = _sut.Monitor();

            // Act
            _sut.ConfirmPassword = "thesmile";

            // Assert
            monitor.Should().RaisePropertyChangeFor(vm => vm.ConfirmPassword);
            monitor.Should().NotRaisePropertyChangeFor(vm => vm.Username);
            monitor.Should().NotRaisePropertyChangeFor(vm => vm.Password);
            monitor.Should().NotRaisePropertyChangeFor(vm => vm.Email);
            monitor.Should().NotRaisePropertyChangeFor(vm => vm.Name);
        }

        [Fact]
        public void SignInCommand_GoTo_LoginViewModel()
        {
            // Arrange
            _navigationServiceMock.Setup(mock => mock.Navigate<LoginViewModel>(It.IsAny<IMvxBundle>()))
                .Returns(Task.CompletedTask);

            // Act
            _sut.SignInCommand.Execute();

            // Assert
            _navigationServiceMock.Verify(mock => mock.Navigate<LoginViewModel>(It.IsAny<IMvxBundle>()), Times.Once);

        }
    }
}
