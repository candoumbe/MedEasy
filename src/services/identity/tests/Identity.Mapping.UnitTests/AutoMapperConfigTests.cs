namespace Identity.Mapping.UnitTests
{
    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    [Feature("Mapping")]
    [Feature("Identity")]
    public class AutoMapperConfigTests
    {
        [Fact]
        public void IsValid() => AutoMapperConfig.Build().AssertConfigurationIsValid();
    }
}
