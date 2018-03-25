using System;
using Xunit;
using Xunit.Categories;

namespace Agenda.Mapping.UnitTests
{
    [UnitTest]
    [Feature("Agenda")]
    public class AutoMapperConfigTests
    {

        [Fact]
        public void Mapping_Is_Valid() => AutoMapperConfig.Build().AssertConfigurationIsValid();
    }
}
