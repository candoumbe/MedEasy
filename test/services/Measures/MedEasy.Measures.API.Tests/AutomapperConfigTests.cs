using Measures.Mapping;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Measures.API.Tests
{
    public class AutomapperConfigTests
    {


        [Fact]
        public void IsValid() => AutoMapperConfig.Build().CreateMapper().ConfigurationProvider.AssertConfigurationIsValid();
    }
}
