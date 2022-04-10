namespace Identity.CQRS.UnitTests.Commands
{
    using FluentAssertions;

    using Identity.CQRS.Commands;
    using Identity.ValueObjects;

    using MedEasy.CQRS.Core.Commands;
    using MedEasy.CQRS.Core.Commands.Results;

    using System;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    [UnitTest]
    [Feature("Authentication")]
    [Feature("JWT")]
    public class InvalidateAccessTokenByUsernameCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public InvalidateAccessTokenByUsernameCommandTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        public void Dispose()
        {
            _outputHelper = null;
        }

        [Fact]
        public void IsCommand() => typeof(InvalidateAccessTokenByUsernameCommand).Should()
            .Implement<ICommand<Guid, UserName, InvalidateAccessCommandResult>>();

        [Fact]
        public void Ctor_Is_Valid()
        {
            InvalidateAccessTokenByUsernameCommand instance = new(UserName.From("thejoker"));
            // Assert
            instance.Id.Should()
                .NotBeEmpty();
        }

        [Fact]
        public void Given_param_is_Empty_Constructor_should_throw_ArgumentException()
        {
            // Act
            Action action = () => new InvalidateAccessTokenByUsernameCommand(UserName.Empty);

            // Assert
            action.Should()
                .Throw<ArgumentException>($"{nameof(UserName)} is empty.").Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }
    }
}
