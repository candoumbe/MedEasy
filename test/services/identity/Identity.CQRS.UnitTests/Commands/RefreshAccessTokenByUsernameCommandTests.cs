﻿using FluentAssertions;
using Identity.CQRS.Commands;
using Identity.DTO;
using MedEasy.CQRS.Core.Commands;
using MedEasy.CQRS.Core.Commands.Results;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Identity.CQRS.UnitTests.Commands
{
    public class RefreshAccessTokenByUsernameCommandTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public RefreshAccessTokenByUsernameCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void IsCommand() => typeof(RefreshAccessTokenByUsernameCommand).Should()
            .Implement<ICommand<Guid, (string username, string expiredAccessToken, string refreshToken, JwtSecurityTokenOptions accessTokenOptions), Option<BearerTokenInfo, RefreshAccessCommandResult>>>();


        public static IEnumerable<object[]> InvalidCtorCases
        {
            get
            {
                yield return new object[] { null, null, null, null, "all parameters are null"};
                yield return new object[] { string.Empty, "header-access.payload.signature", "header-refresh.payload.signature", new JwtSecurityTokenOptions(), "username is empty" };
                yield return new object[] { "   ", "header-access.payload.signature", "header-refresh.payload.signature", new JwtSecurityTokenOptions(), "username is whitespace" };
                yield return new object[] { "thejoker", string.Empty, "header-refresh.payload.signature", new JwtSecurityTokenOptions(), "expiredAccessToken is empty" };
                yield return new object[] { "thejoker", "  ", "header-refresh.payload.signature", new JwtSecurityTokenOptions(), "expiredAccessToken is whitespace" };
                yield return new object[] { "thejoker", null, "header-refresh.payload.signature", new JwtSecurityTokenOptions(), "expiredAccessToken is null" };
                yield return new object[] { "thejoker", "header-access.payload.signature", "  ", new JwtSecurityTokenOptions(), "refresh token is whitespace" };
                yield return new object[] { "thejoker", "header-access.payload.signature", null, new JwtSecurityTokenOptions(), "refresh token is null" };
                yield return new object[] { "thejoker", "header-access.payload.signature", string.Empty, new JwtSecurityTokenOptions(), "refresh token is empty" };
                yield return new object[] { "thejoker", "header-access.payload.signature", "header-refresh.payload.signature", null, "accessTokenOptions is null" };
            }
        }

        [Theory]
        [MemberData(nameof(InvalidCtorCases))]
        public void Ctor_Throws_ArgumentNullException(string username, string expiredAccessToken, string refreshToken, JwtSecurityTokenOptions accessTokenOptions, string reason)
        {

            _outputHelper.WriteLine($"Parameters : {new { username, expiredAccessToken, refreshToken, accessTokenOptions }.Stringify()}");

            // Act
            Action action = () => new RefreshAccessTokenByUsernameCommand((username, expiredAccessToken, refreshToken, accessTokenOptions));

            // Assert
            action.Should()
                .Throw<ArgumentException>(reason).Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();

        }
    }
}