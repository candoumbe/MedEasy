using System;
using Xunit.Abstractions;
using FluentAssertions;
using Xunit;
using Identity.CQRS.Queries.Accounts;
using Xunit.Categories;

namespace Identity.CQRS.UnitTests.Queries.Accounts
{
    [UnitTest]
    [Feature("Accounts")]
    public class GetOneAccountByUsernameAndPasswordQueryTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        public GetOneAccountByUsernameAndPasswordQueryTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        public void Dispose() => _outputHelper = null;

        [Fact]
        public void GivenNullParameter_Ctor_Throws_ArgumentNullException()
        {
            // Act
            Action action = () => new GetOneAccountByUsernameAndPasswordQuery(null);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }
    }
}
