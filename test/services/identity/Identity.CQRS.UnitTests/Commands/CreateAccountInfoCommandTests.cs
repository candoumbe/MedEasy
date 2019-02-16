using FluentAssertions;
using Identity.CQRS.Commands.Accounts;
using Identity.DTO;
using System;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Identity.CQRS.UnitTests.Commands.Accounts
{
    [UnitTest]
    public class CreateAccountInfoForPatientIdCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public CreateAccountInfoForPatientIdCommandTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        public void Dispose()
        {
            _outputHelper = null;
        }

        [Fact]
        public void Ctor_Is_Valid()
        {
            CreateAccountInfoCommand instance = new CreateAccountInfoCommand(new NewAccountInfo()
            {
            });

            // Assert
            instance.Id.Should()
                .NotBeEmpty();
        }

        [Fact]
        public void Ctor_Throws_ArgumentNullException()
        {
            // Act
            Action action = () => new CreateAccountInfoCommand(null);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }
    }
}