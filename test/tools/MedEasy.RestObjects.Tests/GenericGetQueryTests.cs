namespace MedEasy.Tests.RestObjects
{
    using FluentAssertions;

    using MedEasy.RestObjects;

    using Xunit;

    public class GenericGetQueryTests
    {
        [Fact]
        public void DefaultCtor()
        {
            PaginationConfiguration getQuery = new();

            getQuery.PageSize.Should().Be(PaginationConfiguration.DefaultPageSize);
            getQuery.Page.Should().Be(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        [InlineData(0)]
        public void PageSize(int input)
        {
            PaginationConfiguration query = new() { PageSize = input };
            query.PageSize.Should()
                .Be(input, $"because getting the value of {nameof(PaginationConfiguration.PageSize)} with <{input}> should output <{input}> ");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        [InlineData(0)]
        public void Page(int input)
        {
            PaginationConfiguration query = new() { Page = input };
            query.Page.Should()
                .Be(input, $"because getting the value of {nameof(PaginationConfiguration.Page)} with <{input}> should output <{input}> ");
        }
    }
}
