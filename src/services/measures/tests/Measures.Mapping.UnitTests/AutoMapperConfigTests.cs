namespace Measures.Mapping.UnitTests
{
    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    public class AutoMapperConfigTests
    {
        [Fact]
        public void IsValid() => AutoMapperConfig.Build().AssertConfigurationIsValid();
    }
}
