using FluentAssertions;
using FluentAssertions.Events;
using MedEasy.Mobile.Core.Services;
using MedEasy.Mobile.Core.ViewModels;
using MedEasy.Mobile.Core.ViewModels.Base;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xunit;
using Xunit.Categories;
using static Moq.MockBehavior;

namespace MedEasy.Mobile.UnitTests.Core.ViewModels
{
    [UnitTest]
    [Feature("Mobile")]
    public class LostPasswordViewModelTests : IDisposable
    {
        private Mock<INavigatorService> _navigationServiceMock;
        private LostPasswordViewModel _sut;

        public LostPasswordViewModelTests()
        {
            _navigationServiceMock = new Mock<INavigatorService>(Strict);
            _sut = new LostPasswordViewModel(_navigationServiceMock.Object);
        }

        public void Dispose()
        {
            _sut = null;
        }

        [Fact]
        public void IsViewModel() => typeof(ViewModelBase).IsAssignableFrom(_sut.GetType())
            .Should().BeTrue();

        [Fact]
        public void SetEmail_Triggers_PropertyChangeEvent()
        {
            // Arrange
            IMonitor<LostPasswordViewModel> vmMonitor = _sut.Monitor();
            IMonitor<Command> resetPasswordCmdMonitor = _sut.ResetPasswordCommand.Monitor();

            // Act
            _sut.Email = "bruce@wayne-entreprise.com";

            // Assert
            vmMonitor.Should()
                .RaisePropertyChangeFor(vm => vm.Email);
            resetPasswordCmdMonitor.Should()
                .Raise(nameof(Command.CanExecuteChanged));

        }

        [Theory]
        [InlineData(null, false, "Email is null")]
        [InlineData("", false, "Email is empty")]
        [InlineData("   ", false, "Email is whitespace only")]
        [InlineData("bruce", true, "Email is username")]
        [InlineData("bruce@wayne-entreprise.fr", true, "Email is username")]
        public void ResetPasswrordCommand_Can_Execute_DependsOn_Email(string email, bool canExecute, string reason)
        {
            // Arrange
            _sut.Email = email;

            // Act
            bool actualCanExecute = _sut.ResetPasswordCommand.CanExecute(null);

            // Assert
            actualCanExecute.Should()
                .Be(canExecute, reason);

        }

        [Fact]
        public void ResetPasswordCommand_Causes_View_ToDisappear()
        {
            // Arrange
            _navigationServiceMock.Setup(mock => mock.PopModalAsync())
                .Returns(Task.CompletedTask);

            // Act
            _sut.ResetPasswordCommand.Execute(default);

            // Assert
            _navigationServiceMock.Verify(mock => mock.PopModalAsync(), Times.Once);
        }
    }
}
