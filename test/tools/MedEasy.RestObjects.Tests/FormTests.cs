using FluentAssertions;
using Xunit;

namespace MedEasy.RestObjects.Tests
{
    public class FormTests
    {
        [Fact]
        public void DefaultConstructorSetItemsToEmpty() => new Form().Items
            .Should().BeEmpty();
    }
}
