using Blazored.LocalStorage;

using FluentAssertions;

using Identity.Models;
using Identity.Models.v2;
using MedEasy.Web.Pages;
using MedEasy.Web.Services.Identity;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Moq;
using Refit;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Xunit;
using Xunit.Categories;

using static Moq.MockBehavior;

namespace MedEasy.Web.UnitTests.Pages
{
    [UnitTest]
    [Feature(nameof(Login))]
    public class LoginPageTests
    {
        private readonly MockRepository _mockRepository;
        private readonly Mock<NavigationManager> _navigationServiceMock;
        private readonly Mock<IIdentityApi> _loginServiceMock;
        private readonly Mock<ILocalStorageService> _localStorageService;
        private LoginBase _sut;

        public LoginPageTests()
        {
            _mockRepository = new MockRepository(Strict);

            _navigationServiceMock = _mockRepository.Create<NavigationManager>(Loose);

            _loginServiceMock = _mockRepository.Create<IIdentityApi>();
            _localStorageService = _mockRepository.Create<ILocalStorageService>();

            _sut = new LoginBase
            {
                NavigationService = _navigationServiceMock.Object,
                Loginservice = _loginServiceMock.Object,
                LocalStorageService = _localStorageService.Object
            };
        }

        [Fact]
        public void Is_login_page()
        {
            typeof(LoginBase).Should()
                             .HaveDefaultConstructor().And
                             .HaveProperty<bool>("IsConnecting").And
                             .HaveProperty<LoginModel>("Model").And
                             .HaveProperty<bool>("HasErrorToDisplay").And
                             .HaveProperty<bool>("CanConnect").And
                             .HaveMethod("Connect", Enumerable.Empty<Type>()).And
                             .HaveMethod("GoToRegister", Enumerable.Empty<Type>());
        }

        [Fact]
        public void Ctor_builds_a_valid_instance()
        {
            // Assert
            _sut.Model.Should()
                .NotBeNull();
            _sut.Model.Name.Should()
                .BeNull();
            _sut.Model.Password.Should()
                .BeNull();
            _sut.CanConnect.Should().BeFalse();
            _sut.IsConnecting.Should()
                .BeFalse();
        }

        [Fact]
        public async Task Connect_redirect_to_home_when_successfully_logged_in()
        {
            // Arrange
            _sut.Model.Name = "jdoe@home.fr";
            _sut.Model.Password = "a_secret_word";

            BearerTokenModel token = new BearerTokenModel
            {
                AccessToken = new TokenModel { Token = "access_token" },
                RefreshToken = new TokenModel { Token = "refresh_token" }
            };

            HttpResponseMessage responseMessage = new HttpResponseMessage();

            _loginServiceMock.Setup(mock => mock.Login(It.IsNotNull<LoginModel>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new ApiResponse<BearerTokenModel>(responseMessage, token));

            _localStorageService.Setup(mock => mock.SetItemAsync(It.IsAny<string>(), It.IsAny<object>()))
                                .Returns(Task.CompletedTask);


            // Act
            await _sut.Connect()
                      .ConfigureAwait(false);

            // Assert
            _loginServiceMock.Verify(mock => mock.Login(It.IsNotNull<LoginModel>(), It.IsAny<CancellationToken>()), Times.Once);
            _loginServiceMock.Verify(mock => mock.Login(It.Is<LoginModel>(login => login.Name == _sut.Model.Name && login.Password == _sut.Model.Password), It.IsAny<CancellationToken>()), Times.Once);

            _navigationServiceMock.Verify(mock => mock.NavigateTo(It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
            _navigationServiceMock.VerifyNoOtherCalls();

            
        }
    }
}
