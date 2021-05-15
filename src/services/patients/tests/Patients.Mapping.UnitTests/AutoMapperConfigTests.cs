namespace Patients.Mapping.UnitTests
{
    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    [Feature("Patients")]
    [Feature("Mapping")]
    public class AutoMapperConfigTests
    {
        [Fact]
        public void IsValid() => AutoMapperConfig.Build().AssertConfigurationIsValid();
    }
}
