using System;
using Xunit.Abstractions;
using FluentAssertions;
using Xunit;
using Identity.CQRS.Queries.Accounts;

namespace Identity.CQRS.UnitTests.Queries.Accounts
{
    public class GetOneAccountByIdQueryTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        public GetOneAccountByIdQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;            
        }

        public void Dispose() => _outputHelper = null;

        [Fact]
        public void GivenNullParameter_Ctor_Throws_ArgumentNullException()
        {
            // Act
            Action action = () => new GetAccountInfoByIdQuery(id : default);

            // Assert
            action.Should()
                .Throw<ArgumentException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
            
        }

    }
}
