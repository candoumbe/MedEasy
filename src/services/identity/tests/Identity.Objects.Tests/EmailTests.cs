namespace Identity.Objects.Tests
{
    using FluentAssertions;

    using Identity.ValueObjects;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    public class EmailTests
    {
        [Theory]
        [InlineData("username", "Missing '@{provider}.{domain}' part")]
        [InlineData("username@provider", "Missing '{domain}' part")]
        [InlineData("@provider.com", "Missing '{username}' part")]
        public void Given_invalid_input_Create_should_throw_InvalidOperationException(string input, string reason)
        {
            // Act
            Action callingEmailCreate = () => Email.From(input);

            // Assert
            callingEmailCreate.Should()
                              .Throw<InvalidOperationException>(reason);
        }
    }
}
