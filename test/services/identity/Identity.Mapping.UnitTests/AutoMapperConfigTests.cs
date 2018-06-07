using Xunit;
using Xunit.Categories;

namespace Identity.Mapping.UnitTests
{
    [UnitTest]
    [Feature("Mapping")]
    public class AutoMapperConfigTests
    {
        [Fact]
        public void IsValid() => AutoMapperConfig.Build().AssertConfigurationIsValid();
    }
}
