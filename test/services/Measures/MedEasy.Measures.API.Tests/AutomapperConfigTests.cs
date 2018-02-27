using Measures.Mapping;
using Xunit;
using Xunit.Categories;

namespace Measures.API.Tests
{
    [UnitTest]
    public class AutomapperConfigTests
    {


        [Fact]
        public void IsValid() => AutoMapperConfig.Build().CreateMapper().ConfigurationProvider.AssertConfigurationIsValid();
    }
}
