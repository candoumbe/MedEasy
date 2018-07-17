using FluentAssertions;
using Identity.CQRS.Queries;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Identity.CQRS.UnitTests.Queries
{
    public class HashPasswordQueryTests
    {
        private ITestOutputHelper _outputHelper;
        public HashPasswordQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public void Dispose() => _outputHelper = null;

        [Fact]
        public void GivenNullParameter_Ctor_Throws_ArgumentNullException()
        {
            // Act
            Action action = () => new HashPasswordQuery(null);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();

        }
    }
}
