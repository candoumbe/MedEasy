namespace MedEasy.RestObjects.Tests
{
    using FluentAssertions;

    using Xunit;

    public class FormTests
    {
        [Fact]
        public void DefaultConstructorSetItemsToEmpty() => new Form().Items
            .Should().BeEmpty();
    }
}
