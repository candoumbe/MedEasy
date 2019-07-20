using Xunit;
using Xunit.Categories;

namespace Measures.Mapping.UnitTests
{
    [UnitTest]
    public class AutoMapperConfigTests
    {
        [Fact]
        public void IsValid() => AutoMapperConfig.Build().AssertConfigurationIsValid();
    }
}
