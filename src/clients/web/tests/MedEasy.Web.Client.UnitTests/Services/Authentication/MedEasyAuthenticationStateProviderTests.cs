using FluentAssertions;
using Identity.Models.Auth;
using MedEasy.Web.Client.Services;
using MedEasy.Web.Client.Services.Authentication;
using MedEasy.Web.Client.Services.Identity;
using Microsoft.AspNetCore.Components;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Moq.MockBehavior;
using Optional;

namespace MedEasy.Web.Client.UnitTests.Services.Authentication
{
    [UnitTest]
    public class MedEasyAuthenticationStateProviderTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private Mock<IIdentityApi> _identityApiMock;
        private Mock<ITokenService> _tokenServiceMock;
        private MedEasyAuthenticationStateProvider _sut;

        public MedEasyAuthenticationStateProviderTests(ITestOutputHelper outputHelper)
        {
            _identityApiMock = new Mock<IIdentityApi>(Strict);
            _tokenServiceMock = new Mock<ITokenService>(Strict);
            _sut = new MedEasyAuthenticationStateProvider(_identityApiMock.Object, _tokenServiceMock.Object);
            _outputHelper = outputHelper;
        }


        public static IEnumerable<object[]> CtorThrowsArgumentNullExceptionCases
        {
            get
            {
                yield return new object[] { null, Mock.Of<ITokenService>()};
                yield return new object[] { Mock.Of<IIdentityApi>(), null };
            }
        }

        [Theory]
        [MemberData(nameof(CtorThrowsArgumentNullExceptionCases))]
        public void Ctor_With_Null_Parameter_Throws_ArgumentNullException(IIdentityApi identityApi, ITokenService tokenService)
        {
            // Act
            Action action = () => new MedEasyAuthenticationStateProvider(identityApi, tokenService);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        public static IEnumerable<object[]> GetAuthenticationStateCases
        {
            get
            {
                {
                    var refreshToken = new BearerTokenModel()
                    {

                    };

                    yield return new object[] {
                        refreshToken ,
                        (Expression<Func<AuthenticationState, bool>>)(state => state != null),
                        "The token is not null"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetAuthenticationStateCases))]
        public async Task GetAuthenticationState(BearerTokenModel token, Expression<Func<AuthenticationState, bool>> authenticationStateExpectation, string reason)
        {
            // Arrange
            _tokenServiceMock.Setup(mock => mock.GetToken())
                .ReturnsAsync(Option.Some(token));

            // Act
            AuthenticationState state = await _sut.GetAuthenticationStateAsync()
                .ConfigureAwait(false);

            // Assert
            _tokenServiceMock.Verify(mock => mock.GetToken(), Times.Once);
        }

    }
}
