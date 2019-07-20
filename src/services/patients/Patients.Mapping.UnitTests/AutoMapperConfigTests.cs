using Xunit;
using Xunit.Categories;

namespace Patients.Mapping.UnitTests
{
    [UnitTest]
    [Feature("Patients")]
    [Feature("Mapping")]
    public class AutoMapperConfigTests
    {
        [Fact]
        public void IsValid() => AutoMapperConfig.Build().AssertConfigurationIsValid();
    }
}
