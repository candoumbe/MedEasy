namespace Identity.CQRS.UnitTests.Queries.Accounts
{
    using System;
    using Xunit.Abstractions;
    using FluentAssertions;
    using Xunit;
    using Identity.CQRS.Queries.Accounts;
    using Xunit.Categories;

    [UnitTest]
    [Feature("Accounts")]
    public class GetPageOfAccountsQueryTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        public GetPageOfAccountsQueryTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        public void Dispose() => _outputHelper = null;

        [Fact]
        public void GivenNullParameter_Ctor_Throws_ArgumentNullException()
        {
            // Act
            Action action = () => new GetPageOfAccountsQuery(null);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }
    }
}
