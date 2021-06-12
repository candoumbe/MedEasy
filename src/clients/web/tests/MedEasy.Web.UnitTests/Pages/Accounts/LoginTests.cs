namespace MedEasy.Web.UnitTests.Pages.Accounts
{
    using AngleSharp.Dom;

    using Blazorise;
    using Blazorise.Bootstrap;
    using Blazorise.Icons.FontAwesome;

    using BlazorStorage.Extensions;
    using BlazorStorage.Interfaces;

    using Bunit;

    using FsCheck;

    using Identity.Models.v1;

    using MedEasy.Web.Apis.Identity.Interfaces;
    using MedEasy.Web.Constants;
    using MedEasy.Web.Pages.Accounts;

    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using Refit;

    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using Xunit.Categories;

    using static Moq.It;
    using static Moq.MockBehavior;

    [UnitTest]
    public class LoginTests
    {
        private readonly Mock<IIdentityApi> _identityApiMock;
        private readonly Mock<ISessionStorage> _storageMock;
        private readonly Login _sut;
        private readonly Mock<NavigationManager> _navigationManagerMock;

        public LoginTests()
        {
            _navigationManagerMock = new(Strict);
            _identityApiMock = new(Strict);
            _storageMock = new(Strict);
            _sut = new Login(_storageMock.Object, _identityApiMock.Object, _navigationManagerMock.Object);
        }

        [FsCheck.Xunit.Property]
        public Property Given_Username_and_password_CanSubmit_should_depends_on_them(string username, string password)
        {
            // Arrange
            _sut.LoginInfo.Username = username;
            _sut.LoginInfo.Password = password;

            // Act
            bool actual = _sut.CanSubmit();

            // Assert
            return actual.ToProperty().When(!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password));
        }

        [FsCheck.Xunit.Property]
        public void Given_identity_api_created_the_account_Connect_should_store_JWT(NonWhiteSpaceString username,
                                                                                    NonWhiteSpaceString password,
                                                                                    BearerTokenModel tokenModel)
        {
            // Arrange
            using TestContext context = new();

            context.Services.AddBlazorise(options => options.ChangeTextOnKeyPress = true)
                  .AddBootstrapProviders()
                  .AddFontAwesomeIcons();

            _identityApiMock.Setup(mock => mock.Connect(IsAny<LoginModel>(), default))
                            .ReturnsAsync(new ApiResponse<BearerTokenModel>(new(HttpStatusCode.Created),
                                                                            tokenModel,
                                                                            new RefitSettings()));
            _storageMock.Setup(mock => mock.SetItem(IsAny<string>(), IsAny<BearerTokenModel>()))
                        .Returns(ValueTask.CompletedTask);

            context.Services.AddSingleton<IIdentityApi>(_identityApiMock.Object);
            context.Services.AddSingleton<ISessionStorage>(_storageMock.Object);

            IRenderedComponent<Login> sut = context.RenderComponent<Login>(form => form.Add(x => x.LoginInfo,
                                                                                            new LoginModel
                                                                                            {
                                                                                                Username = username.Item,
                                                                                                Password = password.Item
                                                                                            }));

            IElement connectButton = sut.Find("btn-connect");

            // Act
            connectButton.Click();

            // Assert
            _identityApiMock.Verify(mock => mock.Connect(Is<LoginModel>(login => login.Username == username.Item
                                                                                 &&
                                                                                 login.Password == password.Item),
                                                         IsAny<CancellationToken>()));
            _identityApiMock.VerifyNoOtherCalls();

            _storageMock.Verify(mock => mock.SetItem(Is<string>(key => key == StorageKeyNames.Token),
                                                     Is<BearerTokenModel>(tokenToStore => tokenModel == tokenToStore)));
            _storageMock.VerifyNoOtherCalls();

            _navigationManagerMock.Verify(mock => mock.NavigateTo("/", false));
            _navigationManagerMock.VerifyNoOtherCalls();
        }
    }
}
