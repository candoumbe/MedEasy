using FluentAssertions;
using Identity.Models.Auth;
using Identity.Models.Auth.v1;
using MedEasy.Web.Accounts.Services;
using MedEasy.Web.Accounts.Services.Identity;
using Microsoft.AspNetCore.Components;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Moq.MockBehavior;

namespace MedEasy.Web.Accounts.Pages.UnitTests
{
    public class LoginTests
    {
        private Login _sut;
        private Mock<ITokenService> _tokenServiceMock;
        private Mock<IIdentityApi> _identityApiMock;
        private Mock<NavigationManager> _navigationManagerMock;

        public LoginTests()
        {
            _tokenServiceMock = new Mock<ITokenService>(Strict);
            _identityApiMock = new Mock<IIdentityApi>(Strict);
            _navigationManagerMock = new Mock<NavigationManager>(Strict);

            _sut = new Login
            {
                NavigationManager = _navigationManagerMock.Object,
                TokenService = _tokenServiceMock.Object,
                IdentityApi = _identityApiMock.Object
            };
        }

        [Fact]
        public void ComponentAsValidStructure() => typeof(Login).Should()
            .NotBeAbstract().And
            .BeAssignableTo<LoginComponentBase>().And
            .HaveProperty<string>(nameof(Login.ViewModel.Username)).And
            .HaveProperty<string>(nameof(Login.ViewModel.Password));

        [Fact(Skip = "Unable to unit test component for now")]
        public async Task Connect_Should_Save_Token_When_Connection_Successfull()
        {
            // Arrange
            string username = $"john_doe_{Guid.NewGuid()}";
            string password = $"a_strong_password_{Guid.NewGuid()}";

            IDictionary<string, object> viewData = new Dictionary<string, object>
            {
                [nameof(_sut.ViewModel)] = new LoginModel { Username= username, Password = password }
            };

            await _sut.SetParametersAsync(ParameterView.FromDictionary(viewData))
                .ConfigureAwait(false);

            // Act
            await _sut.Connect()
                .ConfigureAwait(false);

            // Assert
            _identityApiMock.Verify(mock => mock.Login(It.IsAny<LoginModel>(), It.IsAny<CancellationToken>()), Times.Once);
            _identityApiMock.Verify(mock => mock.Login(It.Is<LoginModel>(model => model.Username == username && model.Password == password), It.IsAny<CancellationToken>()), Times.Once);
            _identityApiMock.VerifyNoOtherCalls();

            _tokenServiceMock.Verify(mock => mock.SaveToken(It.IsNotNull<BearerTokenModel>()), Times.Once);
            _tokenServiceMock.VerifyNoOtherCalls();
        }
    }
}
